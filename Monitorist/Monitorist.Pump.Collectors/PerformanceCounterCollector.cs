using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Monitorist.Pump.Core;

namespace Monitorist.Pump.Collectors
{
    public class PerformanceCounterCollector : CollectorBase, ICollector, IDisposable
	{
		public List<System.Diagnostics.PerformanceCounter> Counters { get; set; }

		public PerformanceCounterCollector()
		{
			this.Counters = new List<System.Diagnostics.PerformanceCounter>();
		}

		protected override void Collect()
		{
			this.Counters.ForEach(c =>
			{
				try
				{
					var value = new CounterValueModel();
					value.Value = ((decimal) c.NextValue());
					value.HostName = c.MachineName;
					value.CategoryName = c.CategoryName;
					value.CounterName = c.CounterName;
					value.InstanceName = c.InstanceName;

					this.InputBlock.Post(value);

				}
				catch (Exception ex)
				{
					log.ErrorFormat("Exception occured while collection performanceCounter value. HostName: {0}, CategoryName: {1}, CounterName: {2}, InstanceName: {3}, Exception: {4}", c.MachineName, c.CategoryName, c.CounterName, c.InstanceName, ex.Message);
				}
			});
		}

		public override void ResolveCategories(List<CategoryModel> categories)
		{
			categories.ForEach(c => {
				if (!PerformanceCounterCategory.Exists(c.CategoryName, this.HostName))
				{
					log.WarnFormat("Category can not be found on host. CategoryName: {0} | HostName : {1}", c.CategoryName, this.HostName);
					return;
				} 

				var category = new PerformanceCounterCategory(c.CategoryName, this.HostName);
				
				var instanceNames = category.GetInstanceNames();
				if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance ||
					(category.CategoryType == PerformanceCounterCategoryType.Unknown && instanceNames.Length > 0))
				{
					foreach (var instanceName in instanceNames)
					{
						if (!(c.IsAllInstancesIncluded || c.IncludedInstances.Contains(instanceName)))
						{
							continue;
						}

						foreach (var counter in category.GetCounters(instanceName))
						{
							if (!(c.IsAllCountersIncluded ||  c.IncludedCounters.Contains(counter.CounterName)))
							{
								continue;
							}

							this.Counters.Add(counter);
						}
					}
				}
				else
				{
					foreach (var counter in category.GetCounters())
					{
						if (!(c.IsAllCountersIncluded || c.IncludedCounters.Contains(counter.CounterName)))
						{
							continue;
						}

						this.Counters.Add(counter);
					}
				}
			});

			base.ResolveCategories(categories);
		}

		public void Dispose()
		{
			this.Counters.ForEach(c => { c.Dispose(); });
		}

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
