using System;
using System.Collections.Generic;
using System.Linq;
using Topshelf;
using Topshelf.Logging;

namespace Monitorist.Pump.Service
{
    class Program
    {
        static void Main()
        {
            Configuration.TemplateConfig c = new Configuration.TemplateConfig();
            c.Add("SqlServer", new Configuration.Template { Name = "SqlServer", Poll = 3, IncludedCounters = new List<string>(new string[] { "Memory", "CPU" }) });
            c.Add("IIS", new Configuration.Template { Name = "IIS", Poll = 3, ExcludedCounters = new List<string>(new string[] { "Memory", "CPU" }) });
            c.Add("Exchange", new Configuration.Template { Name = "Exchange", Poll = 3, IncludedInstances = new List<string>(new string[] { "Memory", "CPU" }) });

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(c, Newtonsoft.Json.Formatting.Indented);

            HostFactory.Run(f => {
                f.Service<PumpService>();
                f.SetServiceName("Monitorist.Pump");
                f.SetDisplayName("Monitorist Pump");
                f.SetDescription("Pumps the metrics collectors");

                f.UseLog4Net(".\\configs\\log4net.config", true);

                // can be useful for service cleanup since we will use win32 apis and disposables heavily.
                f.EnableShutdown();
                f.EnablePauseAndContinue();

                f.RunAsNetworkService();
                // f.RunAsPrompt();

                f.StartAutomaticallyDelayed();

                f.EnableServiceRecovery(src => {
                    src.RestartService(0);
                });

                // can be helpful if can not reduce time to get up and running
                //f.SetStartTimeout();

                f.AfterInstall(hs => {
                    // install counterss
                });

                f.AfterRollback(hs => {
                    // if counters are install uninstall them for correct celan up
                });

                f.AfterUninstall(() => {
                    // Uninstall counters
                });
            });
        }
    }
}
