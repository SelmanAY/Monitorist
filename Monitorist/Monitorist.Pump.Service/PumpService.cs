using System;
using Topshelf;
using System.Linq;
using log4net;
using Monitorist.Pump.Service.Configuration;
using Monitorist.Pump.Core;

namespace Monitorist.Pump.Service
{
    class PumpService : ServiceControl
    {
        protected ServiceSettings Settings { get; set; }

        protected ISender Sender { get; set; }
        protected ICollector Collector { get; set; }

        public PumpService()
        {
            this.Settings = ServiceSettings.ParseSettings();
            this.Sender = CreateSender();
            this.Collector = CreateCollector();

            this.Timer = new System.Threading.Timer(new System.Threading.TimerCallback(Test), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

        }

        private void ReadConfigFiles()
        {
            
        }

        public void Test(object state)
        {

        }

        public bool Start(HostControl hostControl)
        {
            log.Info("Service Started");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private System.Threading.Timer Timer { get; set; }

        private ISender CreateSender()
        {
            var senderType = Type.GetType(this.Settings.SenderSettings.SenderType);
            if (senderType == null)
            {
                var senderDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Senders\\");
                var dlls = System.IO.Directory.GetFiles(senderDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
                var loader = new AppDomainToolkit.AssemblyLoader();

                foreach (var dll in dlls)
                {
                    var loadedAssemblies = loader.LoadAssemblyWithReferences(AppDomainToolkit.LoadMethod.LoadBits, dll);

                    var _interface = typeof(Monitorist.Pump.Core.ISender);
                    var type = loadedAssemblies
                        .SelectMany(s => s.GetTypes())
                        .Where(p => _interface.IsAssignableFrom(p) && p.IsClass == true && p.FullName.Equals(this.Settings.SenderSettings.SenderType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (type != null)
                    {
                        senderType = type;
                    }
                }
            }

            if (senderType != null)
            {
                ISender result = Activator.CreateInstance(senderType) as ISender;
                result.Initialize(this.Settings.SenderSettings.SenderConfiguration);

                return result;
            }
            else
            {
                throw new ApplicationException(string.Format("Sender object named \"{0}\" can not be loaded. Can not continue to run.", this.Settings.SenderSettings.SenderType));
            }
        }

        private ICollector CreateCollector()
        {
            var senderType = Type.GetType(this.Settings.CollectorSettings.CollectorType);
            if (senderType == null)
            {
                var senderDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Collectors\\");
                var dlls = System.IO.Directory.GetFiles(senderDirectory, "*.dll", System.IO.SearchOption.TopDirectoryOnly);
                var loader = new AppDomainToolkit.AssemblyLoader();

                foreach (var dll in dlls)
                {
                    var loadedAssemblies = loader.LoadAssemblyWithReferences(AppDomainToolkit.LoadMethod.LoadBits, dll);

                    var _interface = typeof(Monitorist.Pump.Core.ICollector);
                    var type = loadedAssemblies
                        .SelectMany(s => s.GetTypes())
                        .Where(p => _interface.IsAssignableFrom(p) && p.IsClass == true && p.FullName.Equals(this.Settings.CollectorSettings.CollectorType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (type != null)
                    {
                        senderType = type;
                    }
                }
            }

            if (senderType != null)
            {
                ICollector result = Activator.CreateInstance(senderType) as ICollector;
                result.Initialize(this.Settings.CollectorSettings.CollectorConfiguration);

                return result;
            }
            else
            {
                throw new ApplicationException(string.Format("Sender object named \"{0}\" can not be loaded. Can not continue to run.", this.Settings.SenderSettings.SenderType));
            }
        }
    }
}

