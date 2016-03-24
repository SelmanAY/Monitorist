using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
    class ServiceSettings
    {
        private Dictionary<string, Template> Templates { get; set; }

        public SenderConfig SenderSettings { get; private set; }
        public CollectorConfig CollectorSettings { get; private set; }
        public List<Host> Hosts { get; set; }



        private ServiceSettings()
        {
            this.Templates = new Dictionary<string, Template>();
            this.Hosts = new List<Host>();
        }

        internal static ServiceSettings ParseSettings()
        {
            var result = new ServiceSettings();

            result.SenderSettings = ParseSenderSettings();
            result.CollectorSettings = ParseCollectorSettings();
            result.Templates = ParseTemplateSettings();
            result.Hosts = ParseHostsSettings();

            return result;
        }

        private static List<Host> ParseHostsSettings()
        {
            var senderStr = System.IO.File.ReadAllText(string.Format(HOSTS_CONFIG_FILE, System.Environment.CurrentDirectory));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Host>>(senderStr);
        }

        private static Dictionary<string, Template> ParseTemplateSettings()
        {
            var result = new Dictionary<string, Template>();

            var senderStr = System.IO.File.ReadAllText(string.Format(TEMPLATES_CONFIG_FILE, System.Environment.CurrentDirectory));
            var templates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Template>>(senderStr);

            templates.ForEach(t => result.Add(t.Name, t));

            return result;
        }

        private static CollectorConfig ParseCollectorSettings()
        {
            var result = new CollectorConfig();

            var senderStr = System.IO.File.ReadAllText(string.Format(COLLECTOR_CONFIG_FILE, System.Environment.CurrentDirectory));
            var json = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(senderStr, new { CollectorType = "", CollectorConfiguration = new object() });

            result.CollectorType = json.CollectorType;
            result.CollectorConfiguration = Newtonsoft.Json.JsonConvert.SerializeObject(json.CollectorConfiguration);

            return result;
        }

        private static SenderConfig ParseSenderSettings()
        {
            var result = new SenderConfig();

            var senderStr = System.IO.File.ReadAllText(string.Format(SENDER_CONFIG_FILE, System.Environment.CurrentDirectory));
            var json = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(senderStr, new { SenderType = "", SenderConfiguration = new object() });

            result.SenderType = json.SenderType;
            result.SenderConfiguration = Newtonsoft.Json.JsonConvert.SerializeObject(json.SenderConfiguration);

            return result;
        }

        private const string SENDER_CONFIG_FILE = "{0}\\configs\\sender.json";
        private const string COLLECTOR_CONFIG_FILE = "{0}\\configs\\collector.json";
        private const string TEMPLATES_CONFIG_FILE = "{0}\\configs\\templates.json";
        private const string HOSTS_CONFIG_FILE = "{0}\\configs\\hosts.json";
    }
}
