using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
    public class Host
    {
        public string HostName { get; set; }
        public int SampleInterval { get; set; }
        public List<string> Templates { get; set; }
        public List<Template> Categories { get; private set; }

        public Host()
        {
            this.Templates = new List<string>();
            this.Categories = new List<Template>();
        }
    }
}
