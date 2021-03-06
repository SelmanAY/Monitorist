﻿using System;
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
            HostFactory.Run(f => {
                f.Service<PumpService>();
                f.SetServiceName("Monitorist.Pump");
                f.SetDisplayName("Monitorist Pump");
                f.SetDescription("Pumps the metrics collectors");

                f.SetStartTimeout(TimeSpan.FromSeconds(60));

                f.UseLog4Net(".\\configs\\log4net.config", true);

                // can be useful for service cleanup since we will use win32 apis and disposables heavily.
                f.EnableShutdown();
                f.EnablePauseAndContinue();

                // f.RunAsNetworkService();
                f.RunAsPrompt();

                f.StartAutomatically();

                f.EnableServiceRecovery(src => {
                    src.RestartService(1);
                });
                
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
