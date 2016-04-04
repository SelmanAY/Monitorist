using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Monitorist.Pump.Core;

namespace Monitorist.Pump.Core
{
	public class CollectorBase : ICollector
	{
		public string HostName { get; protected set; }
		public int SampleInterval { get; protected set; }
		protected System.Timers.Timer Timer { get; set; }

		private List<CategoryModel> Categories { get; set; }

		private bool IsCategoriesResolved { get; set; }

		public BufferBlock<CounterValueModel> InputBlock { get; set; }
		
		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			this.Collect();
		}

		protected virtual void Collect()
		{

		}

		public void AddCategory(string name)
		{
			if (this.IsCategoriesResolved)
			{
				throw new ApplicationException("Can not add to categories because categories are resolved. You should add categories before calling ResolveCategories method");
			}

			this.Categories.Add(new CategoryModel { CategoryName = name });
		}

		public void AddCategory(string name, IEnumerable<string> includedInstances)
		{
			if (this.IsCategoriesResolved)
			{
				throw new ApplicationException("Can not add to categories because categories are resolved. You should add categories before calling ResolveCategories method");
			}

			this.Categories.Add(new CategoryModel { CategoryName = name, IncludedInstances = new List<string>(includedInstances) });
		}

		public void AddCategory(string name, IList<string> includedCounters)
		{
			if (this.IsCategoriesResolved)
			{
				throw new ApplicationException("Can not add to categories because categories are resolved. You should add categories before calling ResolveCategories method");
			}

			this.Categories.Add(new CategoryModel { CategoryName = name, IncludedCounters = new List<string>(includedCounters) });
		}

		public void AddCategory(string name, List<string> includedCounters, List<string> includedInstances)
		{
			if (this.IsCategoriesResolved)
			{
				throw new ApplicationException("Can not add to categories because categories are resolved. You should add categories before calling ResolveCategories method");
			}

			this.Categories.Add(new CategoryModel { CategoryName = name, IncludedCounters = includedCounters, IncludedInstances = includedInstances });
		}

		public virtual void Start()
		{
			this.ResolveCategories(this.Categories);
			this.Timer.Start();
		}

		public virtual void Stop()
		{
			this.Timer.Stop();
		}

		public virtual void Initialize(string collectorConfiguration, string hostName, int sampleInterval, BufferBlock<CounterValueModel> inputBlock)
		{
			this.HostName = hostName;
			this.SampleInterval = sampleInterval;
			this.InputBlock = inputBlock;
			
			this.Timer = new System.Timers.Timer(sampleInterval);
			this.Timer.Elapsed += Timer_Elapsed;
			
			this.Categories = new List<CategoryModel>();
		}

		public virtual void ResolveCategories(List<CategoryModel> categories)
		{
			this.IsCategoriesResolved = true;
		}

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
}
