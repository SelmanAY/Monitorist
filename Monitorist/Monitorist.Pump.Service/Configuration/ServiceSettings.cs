using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitorist.Pump.Service.Configuration
{
    class ServiceSettings
    {
        public SenderConfig SenderSettings { get; private set; }
        public CollectorConfig CollectorSettings { get; set; }

        private ServiceSettings()
        {

        }

        internal static ServiceSettings ParseSettings()
        {
            var result = new ServiceSettings();

            result.SenderSettings = ParseSenderSettings();
            result.CollectorSettings = ParseCollectorSettings();
            

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
    }
}
