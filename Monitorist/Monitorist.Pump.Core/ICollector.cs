using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Monitorist.Pump.Core
{
    public interface ICollector
    {
		string HostName { get; }

		int SampleInterval { get; }

		void Start();

		void Stop();

		/// <summary>
		/// Adds category with all counters and all instances. 
		/// </summary>
		/// <param name="name">Name of category</param>
		void AddCategory(string name);

		/// <summary>
		/// Adds category with specific counters and specific instances. 
		/// </summary>
		/// <param name="name">Name of category</param>
		/// <param name="includedCounters">Names of counters to be monitored</param>
		/// <param name="includedInstances">Names of instances to be monitored</param>
		void AddCategory(string name, List<string> includedCounters, List<string> includedInstances);

		/// <summary>
		/// Adds category with specific counters and all instances of them
		/// </summary>
		/// <param name="name">Name of category</param>
		/// <param name="includedCounters">Names of counters to be monitored</param>
		void AddCategory(string name, IList<string> includedCounters);

		/// <summary>
		/// Adds category with all counters of specific instances. 
		/// </summary>
		/// <param name="name">Name of category</param>
		/// <param name="includedInstances">Names of instances to be monitored</param>
		void AddCategory(string name, IEnumerable<string> includedInstances);
		void Initialize(string collectorConfiguration, string hostName, int sampleInterval, BufferBlock<CounterValueModel> inputBlock);
		void ResolveCategories(List<CategoryModel> categories);
	}
}
