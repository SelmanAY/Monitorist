using System;
using Topshelf;
using System.Linq;
using log4net;
using Monitorist.Pump.Service.Configuration;
using Monitorist.Pump.Core;
using System.Collections.Generic;

namespace Monitorist.Pump.Service
{
    sealed class PumpService : ServiceControl
    {
		private List<HostModel> Hosts { get; set; }
		private ServiceConfig Configs { get; set; }
		private ISender Sender { get; set; }
		private List<ICollector> Collectors { get; set; }
		private System.Threading.Tasks.Dataflow.BufferBlock<CounterValueModel> InputBlock { get; set; }
		
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public System.Timers.Timer StatisticsTimer { get; set; }

		public PumpService()
        {
			try
			{
				log.Info("Service Created");

				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

				this.InputBlock = new System.Threading.Tasks.Dataflow.BufferBlock<CounterValueModel>();

				this.StatisticsTimer = new System.Timers.Timer(1000);
				this.StatisticsTimer.Elapsed += StatisticsTimer_Elapsed;
				this.StatisticsTimer.Start();

			}
			catch (Exception ex)
			{
				log.Error("Exception occured on service constructor", ex);
				throw ex;
			}
		}

		private void StatisticsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			
		}

		public bool Start(HostControl hostControl)
        {
			try
			{
				log.Info("Service Started");
                hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(60));

                this.Collectors = new List<ICollector>();
                this.Configs = ServiceConfig.ParseSettings();

                this.Sender = CreateSender();
                this.Sender.Initialize(this.Configs.SenderConfig.SenderConfiguration, this.InputBlock);

                this.Hosts = this.ResolveHostModels();
                this.Collectors = CreateCollectors();
                
                this.Collectors.ForEach(c => c.Start());
				
				return true;
			}
			catch (Exception ex)
			{
				log.Error("Exception occured on service start", ex);
				return false;
			}
        }

		private List<ICollector> CreateCollectors()
		{
			List<ICollector> result = new List<ICollector>();

			ICollector collector;
			foreach (var host in this.Hosts)
			{
				log.InfoFormat("Creating collector for hostname : {0}", host.HostName);

				collector = CreateCollector(host);

				collector.Initialize(this.Configs.CollectorConfig.CollectorConfiguration, host.HostName, host.SampleInterval, this.InputBlock);
				host.Categories.ForEach(c => {
					var allCounters = c.IncludedCounters == null || c.IncludedCounters.Count == 0;
					var allInstances = c.IncludedInstances == null || c.IncludedInstances.Count == 0;

					if (allCounters && allInstances)
					{
						collector.AddCategory(c.Name);
					}
					else if (allCounters && !allInstances)
					{
						collector.AddCategory(c.Name, c.IncludedInstances);
					}
					else if (!allCounters && allInstances)
					{
						collector.AddCategory(c.Name, c.IncludedCounters);
					}
					else
					{
						collector.AddCategory(c.Name, c.IncludedCounters, c.IncludedInstances);
					}
				});

				result.Add(collector);
			}

			return result;
		}

		private List<HostModel> ResolveHostModels()
		{
			var result = new List<HostModel>();

			this.Configs.HostConfigs.ForEach(h => { result.Add(CreateHostModel(h)); });

			return result;
		}

		/// <summary>
		/// this method combines categories from different templates that are listed in "templates" attribute of objects in hosts.json
		/// then resolves any regexnamed categories in them. 
		/// then if any category listed more than once (from different templates) combines their IncludedCounters and IncludedInstances.
		/// </summary>
		/// <remarks>
		/// !!!! WARNING !!!!
		/// extensive set based operations. 
		/// I can't figure out a good and more readible algortihm to correlate many (mostly nested) lists.
		/// I can think better at set based operations then row based operations. 
		/// i have sacrificed readibility here.
		/// i dont even expect to understand this method in case of a bug stems from here, so pray with for this method won't produce any bugs later.   
		/// </remarks>
		private HostModel CreateHostModel(HostConfig h)
        {
			var result = new HostModel();

			result.HostName = h.HostName;
			result.SampleInterval = h.SampleInterval;

			// all categories within templates of host.
			var allCategories = (from tc in this.Configs.TemplateConfigs
								 join t in h.Templates on tc.Name equals t
								 select tc.Categories).SelectMany(x => x);

			List<CategoryConfig> resolvedRegexCategories = new List<CategoryConfig>();
			
			// resolved categories from regexnamed categies. 
			allCategories.Where(x=>x.IsRegexName).ToList().ForEach(c => {
				var allPerfCats = System.Diagnostics.PerformanceCounterCategory.GetCategories(h.HostName);
				var mathcedCategories = from n in allPerfCats where System.Text.RegularExpressions.Regex.IsMatch(n.CategoryName, c.CategoryName) select new CategoryConfig { CategoryName = n.CategoryName, IncludedCounters = c.IncludedCounters, IncludedInstances = c.IncludedInstances, IsRegexName = false };

				resolvedRegexCategories.AddRange(mathcedCategories);
			});
             
			// combination of regexnamed categories and non regexnamed categories. 
			var resolved = allCategories.Where(x => !x.IsRegexName).Union(resolvedRegexCategories);

			// unicity of resolved categories
			var unicity = from n in resolved group n by n.CategoryName into grp select new { CategoryName = grp.Key, Count = grp.Count() };
			
			// if all the names of resolved categories are unique then return them;
			if (unicity.Where(u => u.Count > 1).Count() == 0)
			{
				resolved.ToList().ForEach(r => result.Categories.Add(new CategoryModel { Name = r.CategoryName, IncludedCounters = r.IncludedCounters, IncludedInstances = r.IncludedInstances }));
			}
			else
			{
                // non duplicate categories
                var nonDuplicates = from n in resolved join x in unicity on n.CategoryName equals x.CategoryName where x.Count == 1 select n;

                // duplicate categories
                var duplicates = new List<CategoryConfig>();

                // check for unicity and combine includedCounters and includedinstance properties of non uniqe categories.
                var duplicate2 = from x in unicity where x.Count > 1 select new { Name = x.CategoryName, Count = x.Count, Categories = from n in resolved where n.CategoryName == x.CategoryName select n };
                duplicate2.ToList().ForEach(d => {
                    duplicates.Add(MergeCategoryConfigs(d.Categories));
                });

                // union duplicate and non duplicate categories and create CategoryModel for all items in resultset.
                nonDuplicates.Union(duplicates).ToList().ForEach(r => {
					result.Categories.Add(new CategoryModel { Name = r.CategoryName, IncludedCounters = r.IncludedCounters, IncludedInstances = r.IncludedInstances });
                });           
            }

			return result;
        }

        private CategoryConfig MergeCategoryConfigs(IEnumerable<CategoryConfig> categories)
        {
            var result = new CategoryConfig();

            // we are pretty sure these two are unique and these lines are safe unless we change the order of steps of algorithm in CreateHostModel method. 
            result.CategoryName = categories.Select(n => n.CategoryName).First();
            result.IsRegexName = categories.Select(n => n.IsRegexName).First();

            if (categories.Any(c => c.IncludedCounters == null || c.IncludedCounters.Count == 0))
            {
                // if any of the IncludedCounters list is null or empty this means collect all the counters then result will be to collect all the counters
                result.IncludedCounters = null;
            }
            else
            {
                // if non of the IncludedCounters list is null or empty this means the result should be unique values in the combination of all lists
                result.IncludedCounters = categories.SelectMany(c => c.IncludedCounters).Distinct().ToList();
            }

            if (categories.Any(c => c.IncludedInstances == null || c.IncludedInstances.Count == 0))
            {
                // if any of the IncludedItems list is null or empty this means collect all the instances then result will be to collect all the instances
                result.IncludedInstances = null;
            }
            else
            {
                // if non of the IncludedInstances list is null or empty this means the result should be unique values in the combination of all lists
                result.IncludedInstances = categories.SelectMany(c => c.IncludedInstances).Distinct().ToList();
            }

            return result;
        }

        public bool Stop(HostControl hostControl)
        {
			this.Collectors.ForEach(c => c.Stop());

            return true;
        }
		
        private ISender CreateSender()
        {
			ISender result = CreateTypeFromLoadedAssemblies<ISender>(this.Configs.SenderConfig.SenderType);
			if (result != null)
			{
				return result;
			}

			result = CreateTypeFromDynamicAssemblies<ISender>(this.Configs.SenderConfig.SenderType);
			if (result != null)
			{
				return result;
			}

			throw new ApplicationException(string.Format("Sender object named \"{0}\" can not be loaded. Can not continue to run.", this.Configs.SenderConfig.SenderType));
        }

        private ICollector CreateCollector(HostModel host)
        {
			ICollector result = CreateTypeFromLoadedAssemblies<ICollector>(this.Configs.CollectorConfig.CollectorType);
			if (result != null)
			{
				return result;
			}

			result = CreateTypeFromDynamicAssemblies<ICollector>(this.Configs.CollectorConfig.CollectorType);
			if (result != null)
			{
				return result;
			}
			
            throw new ApplicationException(string.Format("Sender object named \"{0}\" can not be loaded. Can not continue to run.", this.Configs.SenderConfig.SenderType));
        }

		private T CreateTypeFromDynamicAssemblies<T>(string typeName) where T : class
		{
			System.Type type = null;

			var senderDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Collectors\\");
			var collectorsDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Senders\\");

			var dlls = System.IO.Directory.GetFiles(senderDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly)
						.Union(System.IO.Directory.GetFiles(collectorsDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly));
			var loader = new AppDomainToolkit.AssemblyLoader();

			foreach (var dll in dlls)
			{
				var loadedAssemblies = loader.LoadAssemblyWithReferences(AppDomainToolkit.LoadMethod.LoadBits, dll);

				var _interface = typeof(T);
				type = loadedAssemblies
					.SelectMany(s => s.GetTypes())
					.Where(p => _interface.IsAssignableFrom(p) && p.IsClass == true && p.FullName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

				if (type != null)
				{
					break;
				}
			}

			if (type != null)
			{
				return Activator.CreateInstance(type) as T;
			}
			else
			{
				return null;
			}
		}

		private T CreateTypeFromLoadedAssemblies<T>(string typeName) where T : class
		{
			var type = (from n in AppDomain.CurrentDomain.GetAssemblies()
						from m in n.GetTypes()
						where m.FullName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase) select m).FirstOrDefault();

			if (type != null)
			{
				return Activator.CreateInstance(type) as T;
			}
			else
			{
				return null;
			}
		}

		private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var dllFileName = args.Name.Split(",".ToCharArray())[0] + ".dll";

			var collectorDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Collectors\\");
			if (System.IO.File.Exists(System.IO.Path.Combine(collectorDirectory, dllFileName)))
			{
				return new AppDomainToolkit.AssemblyLoader().LoadAssembly(AppDomainToolkit.LoadMethod.LoadBits, System.IO.Path.Combine(collectorDirectory, dllFileName));
			}

			var senderDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Senders\\");
			if (System.IO.File.Exists(System.IO.Path.Combine(senderDirectory, dllFileName)))
			{
				return new AppDomainToolkit.AssemblyLoader().LoadAssembly(AppDomainToolkit.LoadMethod.LoadBits, System.IO.Path.Combine(senderDirectory, dllFileName));
			}

			return null;
		}
	}
}

