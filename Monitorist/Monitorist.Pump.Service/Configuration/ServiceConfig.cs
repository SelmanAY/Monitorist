using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
    class ServiceConfig
    {
        public SenderConfig SenderConfig { get; private set; }
        public CollectorConfig CollectorConfig { get; private set; }

        public List<TemplateConfig> TemplateConfigs { get; private set; }

        public List<HostConfig> HostConfigs { get; set; }

        private ServiceConfig()
        {
            this.TemplateConfigs = new List<TemplateConfig>();
            this.HostConfigs = new List<HostConfig>();
        }

        internal static ServiceConfig ParseSettings()
        {
            var result = new ServiceConfig();

            result.SenderConfig = ParseSenderSettings();
            result.CollectorConfig = ParseCollectorSettings();
            result.TemplateConfigs = ParseTemplateSettings();
            result.HostConfigs = ParseHostsSettings();

            return result;
        }

        private static List<HostConfig> ParseHostsSettings()
        {
            var senderStr = System.IO.File.ReadAllText(string.Format(HOSTS_CONFIG_FILE, System.Environment.CurrentDirectory));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<HostConfig>>(senderStr);
        }

        private static List<TemplateConfig> ParseTemplateSettings()
        {
            var senderStr = System.IO.File.ReadAllText(string.Format(TEMPLATES_CONFIG_FILE, System.Environment.CurrentDirectory));
            var templates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TemplateConfig>>(senderStr);

            return templates;
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
