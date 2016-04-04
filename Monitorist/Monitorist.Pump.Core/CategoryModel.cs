using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Core
{
	public class CategoryModel
	{
		public string CategoryName { get; set; }
		public List<string> IncludedCounters { get; set; }
		public List<string> IncludedInstances { get; set; }
		public bool IsAllCountersIncluded { get { return this.IncludedCounters.Count == 0; } }
		public bool IsAllInstancesIncluded { get { return this.IncludedInstances.Count == 0; } }

		public CategoryModel()
		{
			this.IncludedCounters = new List<string>();
			this.IncludedInstances = new List<string>();
		}
	}
}
