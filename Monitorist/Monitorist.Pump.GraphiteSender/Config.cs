using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.GraphiteSender
{
	public class Config
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public string MetricPrefix { get; set; }
		public string Protocol { get; set; }
		public int BatchSize { get; set; }
		public List<Transformation> Transformations { get; set; }
	}
}
