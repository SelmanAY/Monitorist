using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
	public class CategoryConfig
	{
		public string CategoryName { get; set; }
		public List<string> IncludedInstances { get; set; }
		public List<string> IncludedCounters { get; set; }
		public bool IsRegexName { get; set; }
	}
}
