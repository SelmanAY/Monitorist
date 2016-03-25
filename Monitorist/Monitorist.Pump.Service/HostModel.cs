using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service
{
	class HostModel
	{
		public string HostName { get; internal set; }
		public int SampleInterval { get; internal set; }
		public List<CategoryModel> Categories { get; set; }

		public HostModel()
		{
			this.Categories = new List<CategoryModel>();
		}
	}

	class CategoryModel
	{
		public string Name { get; set; }
		public List<string> IncludedCounters { get; set; }
		public List<string> IncludedInstances { get; set; }
	}
}
