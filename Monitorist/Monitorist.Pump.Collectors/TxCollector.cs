using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Monitorist.Pump.Core;
using Tx.Windows;

namespace Monitorist.Pump.Collectors
{
    public class TxCollector : CollectorBase, ICollector, IDisposable, IObserver<PerformanceSample>
	{
		public List<string> CounterPaths { get; set; }

		private IDisposable _subscription;

		public TxCollector()
		{
			this.CounterPaths = new List<string>();
		}

		public override void ResolveCategories(List<CategoryModel> categories)
		{
			base.ResolveCategories(categories);

			categories.ForEach(c => {
				if (!PerformanceCounterCategory.Exists(c.CategoryName, this.HostName))
				{
					return;
				}

				var category = new PerformanceCounterCategory(c.CategoryName, this.HostName);
				var instanceNames = category.GetInstanceNames();

				List<string> instances = c.IsAllInstancesIncluded ? new List<string>(new string[] { "*" }) : c.IncludedInstances;
				List<string> counters;

				if (instanceNames.Length > 0)
				{
					counters = c.IsAllCountersIncluded ? new List<string>(category.GetCounters(instanceNames[0]).Select(v => v.CounterName)) : c.IncludedCounters;
					var combination = from n in instances join m in counters on 1 equals 1 select new { Instance = n, Counter = m };
					combination.ToList().ForEach(t => {
						this.CounterPaths.Add(string.Format("\\\\{0}\\{1}({2})\\{3}", this.HostName, c.CategoryName, t.Instance, t.Counter));
					});
				}
				else
				{
					counters = c.IsAllCountersIncluded ? new List<string>(category.GetCounters().Select(v => v.CounterName)) : c.IncludedCounters;
					var combination = from n in instances join m in counters on 1 equals 1 select new { Instance = n, Counter = m };
					combination.ToList().ForEach(t => {
						this.CounterPaths.Add(string.Format("\\\\{0}\\{1}\\{2}", this.HostName, c.CategoryName, t.Counter));
					});
				}
			});
		}

		public override void Initialize(string collectorConfiguration, string hostName, int sampleInterval, BufferBlock<CounterValueModel> inputBlock)
		{
			base.Initialize(collectorConfiguration, hostName, sampleInterval, inputBlock);
		}

		public override void Start()
		{
			base.Start();

			IObservable<PerformanceSample> perfCounters = PerfCounterObservable.FromRealTime(TimeSpan.FromMilliseconds(this.SampleInterval), this.CounterPaths.ToArray());
			_subscription = perfCounters.SubscribeSafe(this);
		}

		public void Dispose()
		{
			_subscription.Dispose();
		}

		public void OnNext(PerformanceSample value)
		{
			var model = new CounterValueModel();
			model.HostName = value.Machine;
			model.CategoryName = value.CounterSet;
			model.CounterName = value.CounterName;
			model.InstanceName = value.Instance;
			model.Value = ((decimal) value.Value);

			this.InputBlock.Post(model);
		}

		public void OnError(Exception error)
		{
			log.Error("Exception occured during collection", error);
		}

		public void OnCompleted()
		{
			log.Info("Collection completed. This should not be happened.");
		}

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
}
