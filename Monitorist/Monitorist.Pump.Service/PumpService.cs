using System;
using Topshelf;
using System.Linq;
using log4net;
using Monitorist.Pump.Service.Configuration;
using Monitorist.Pump.Core;
using System.Collections.Generic;

namespace Monitorist.Pump.Service
{
    class PumpService : ServiceControl
    {
		public List<HostModel> HostData { get; set; }

		protected ServiceConfig Configs { get; set; }

        protected ISender Sender { get; set; }
        protected ICollector Collector { get; set; }

        public PumpService()
        {
			log.Info("Service Created");
			this.Configs = ServiceConfig.ParseSettings();
		}

        public bool Start(HostControl hostControl)
        {
			log.Info("Service Started");
            this.Sender = CreateSender();
            this.Collector = CreateCollector();
			this.HostData = this.ResolveHostModels();

			return true;
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
		/// since I'm a DBA i can't figure out a good and more readible algortihm to correlate many (mostly nested) lists
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
				var allPerfCats = System.Diagnostics.PerformanceCounterCategory.GetCategories();
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
                    result.Categories.Add(new CategoryModel { Name = r.CategoryName, IncludedCounters = r.IncludedCounters, IncludedInstances = r.IncludedInstances })
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
            return true;
        }


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private System.Threading.Timer Timer { get; set; }

        private ISender CreateSender()
        {
            var senderType = Type.GetType(this.Configs.SenderConfig.SenderType);
            if (senderType == null)
            {
                var senderDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Senders\\");
                var dlls = System.IO.Directory.GetFiles(senderDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
                var loader = new AppDomainToolkit.AssemblyLoader();

                foreach (var dll in dlls)
                {
                    var loadedAssemblies = loader.LoadAssemblyWithReferences(AppDomainToolkit.LoadMethod.LoadBits, dll);

                    var _interface = typeof(Monitorist.Pump.Core.ISender);
                    var type = loadedAssemblies
                        .SelectMany(s => s.GetTypes())
                        .Where(p => _interface.IsAssignableFrom(p) && p.IsClass == true && p.FullName.Equals(this.Configs.SenderConfig.SenderType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (type != null)
                    {
                        senderType = type;
                    }
                }
            }

            if (senderType != null)
            {
                ISender result = Activator.CreateInstance(senderType) as ISender;
                result.Initialize(this.Configs.SenderConfig.SenderConfiguration);

                return result;
            }
            else
            {
                throw new ApplicationException(string.Format("Sender object named \"{0}\" can not be loaded. Can not continue to run.", this.Configs.SenderConfig.SenderType));
            }
        }

        private ICollector CreateCollector()
        {
            var senderType = Type.GetType(this.Configs.CollectorConfig.CollectorType);
            if (senderType == null)
            {
                var senderDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Collectors\\");
                var dlls = System.IO.Directory.GetFiles(senderDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
                var loader = new AppDomainToolkit.AssemblyLoader();

                foreach (var dll in dlls)
                {
                    var loadedAssemblies = loader.LoadAssemblyWithReferences(AppDomainToolkit.LoadMethod.LoadBits, dll);

                    var _interface = typeof(Monitorist.Pump.Core.ICollector);
                    var type = loadedAssemblies
                        .SelectMany(s => s.GetTypes())
                        .Where(p => _interface.IsAssignableFrom(p) && p.IsClass == true && p.FullName.Equals(this.Configs.CollectorConfig.CollectorType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (type != null)
                    {
                        senderType = type;
                    }
                }
            }

            if (senderType != null)
            {
                ICollector result = Activator.CreateInstance(senderType) as ICollector;
                result.Initialize(this.Configs.CollectorConfig.CollectorConfiguration);

                return result;
            }
            else
            {
                throw new ApplicationException(string.Format("Sender object named \"{0}\" can not be loaded. Can not continue to run.", this.Configs.SenderConfig.SenderType));
            }
        }
    }
}

