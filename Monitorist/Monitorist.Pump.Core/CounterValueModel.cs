using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Core
{
	public class CounterValueModel
	{
		public string HostName { get; set; }
		public string CategoryName { get; set; }
		public string CounterName { get; set; }
		public string InstanceName { get; set; }
		public decimal Value { get; set; }

		public long TimeStamp { get; set; }
	}
}
