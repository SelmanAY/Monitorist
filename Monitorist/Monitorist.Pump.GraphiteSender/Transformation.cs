using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.GraphiteSender
{
	public class Transformation
	{
		public string Regex { get; set; }
		public string Replace { get; set; }
	}
}
