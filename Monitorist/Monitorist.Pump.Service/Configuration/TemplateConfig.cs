using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
    public class TemplateConfig
    {
		public string Name { get; set; }
		public List<CategoryConfig> Categories { get; set; }
	}
}
