using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Monitorist.Pump.Core;

namespace Monitorist.Pump.Collectors
{
    public class CategoryReaderCollector : CollectorBase, ICollector
	{
		public List<CategoryModelEx> Categories { get; private set; }

		public System.Collections.Hashtable Samples { get; set; }

		private TransformManyBlock<CategoryData, CounterValueModel> ExtractValuesBlock { get; set; }


		public CategoryReaderCollector()
		{
			this.Categories = new List<CategoryModelEx>();
			this.ExtractValuesBlock = new TransformManyBlock<CategoryData, CounterValueModel>(
				new Func<CategoryData, IEnumerable<CounterValueModel>>(ExtractValues)
				, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 100 });

			this.Samples = new System.Collections.Hashtable();
		}

		public override void Initialize(string collectorConfiguration, string hostName, int sampleInterval, BufferBlock<CounterValueModel> inputBlock)
		{
			base.Initialize(collectorConfiguration, hostName, sampleInterval, inputBlock);

			this.ExtractValuesBlock.LinkTo(inputBlock);
		}

		public override void ResolveCategories(List<CategoryModel> categories)
		{
			categories.ForEach(c=> {
				if (!PerformanceCounterCategory.Exists(c.CategoryName, this.HostName))
				{
					return;
				}

				this.Categories.Add(new CategoryModelEx(c, new PerformanceCounterCategory(c.CategoryName, this.HostName)));
			});

			base.ResolveCategories(categories);
		}

		protected override void Collect()
		{
			this.Categories.ForEach(c=> {
				var idcc = c.PerformanceCounterCategory.ReadCategory();
				this.ExtractValuesBlock.Post(new CategoryData(idcc, c.CategoryName));
			});
		}

		internal IEnumerable<CounterValueModel> ExtractValues(CategoryData idcc)
		{
			var idcce = idcc.Data.GetEnumerator();
			while (idcce.MoveNext())
			{
				var idc = idcce.Entry;
				var idce = ((InstanceDataCollection) idc.Value).GetEnumerator();
				while (idce.MoveNext())
				{
					var id = idce.Entry.Value as InstanceData;
					var sample = new PerformanceSample
					{
						MachineName = this.HostName,
						CategoryName = idcc.CategoryName,
						CounterName = ((System.Diagnostics.InstanceDataCollection) idc.Value).CounterName,
						InstanceName = id.InstanceName,
						Sample = id.Sample
					};

					if (this.Samples.Contains(sample.Key))
					{
						var sold = this.Samples[sample.Key] as PerformanceSample;
						var value = ((decimal) CounterSample.Calculate(sold.Sample, sample.Sample));

						this.Samples[sample.Key] = sample;

						yield return new CounterValueModel
						{
							HostName = this.HostName,
							CategoryName = idcc.CategoryName,
							CounterName = ((System.Diagnostics.InstanceDataCollection) idc.Value).CounterName,
							InstanceName = id.InstanceName,
							TimeStamp = sample.Sample.CounterTimeStamp,
							Value = value
						};
					}
					else
					{
						this.Samples.Add(sample.Key, sample);

						yield return new CounterValueModel
						{
							HostName = this.HostName,
							CategoryName = idcc.CategoryName,
							CounterName = ((System.Diagnostics.InstanceDataCollection) idc.Value).CounterName,
							InstanceName = id.InstanceName,
							TimeStamp = sample.Sample.TimeStamp,
							Value = ((decimal) CounterSample.Calculate(sample.Sample))
						};
					}
				}
			}
		}

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal class CategoryData
		{
			public InstanceDataCollectionCollection Data { get; set; }
			public string CategoryName { get; set; }

			public CategoryData(InstanceDataCollectionCollection idcc, string categoryName)
			{
				this.Data = idcc;
				this.CategoryName = categoryName;
			}
		}

		internal class PerformanceSample
		{
			public string MachineName { get; set; }
			public string CategoryName { get; set; }
			public string CounterName { get; set; }
			public string InstanceName { get; set; }
			public CounterSample Sample { get; set; }
			public string Key
			{
				get
				{
					return string.Format("\\\\{0}\\{1}({2})\\{3}",
						this.MachineName,
						this.CategoryName,
						string.IsNullOrEmpty(this.InstanceName) ? "" : this.InstanceName,
						this.CounterName);
				}
			}
		}
	}

	

	public class CategoryModelEx : CategoryModel
	{
		public PerformanceCounterCategory PerformanceCounterCategory { get; set; }

		public CategoryModelEx(CategoryModel c, PerformanceCounterCategory category)
		{
			this.CategoryName = c.CategoryName;
			this.IncludedCounters = c.IncludedCounters;
			this.IncludedInstances = c.IncludedInstances;
			this.PerformanceCounterCategory = category;
		}
	}
}
