using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
    public class Template
    {
        public string Name { get; set; }
        public int Poll { get; set; }
        public List<string> ExcludedInstances { get; set; }
        public List<string> ExcludedCounters { get; set; }
        public List<string> IncludedInstances { get; set; }
        public List<string> IncludedCounters { get; set; }

        public Template()
        {
            this.ExcludedInstances = new List<string>();
            this.ExcludedCounters = new List<string>();

            this.IncludedCounters = new List<string>();
            this.IncludedInstances = new List<string>();
        }
    }
}
