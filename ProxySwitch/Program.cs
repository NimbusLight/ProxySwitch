// <copyright file="Program.cs" company="NimbusLight">
// Copyright (c) NimbusLight. All rights reserved.
// </copyright>

namespace ProxyController
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Win32;
    using Serilog;
    using Serilog.Sinks.SystemConsole.Themes;

    public class Program
    {
        [Option]
        [Required]
        public Status State { get; }

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(@"logs\log-proxy.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .MinimumLevel.Debug()
                .CreateLogger();

            return CommandLineApplication.Execute<Program>(args);
        }

        private void OnExecute()
        {
            using (var registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true))
            {
                int currentProxy = (int)registry.GetValue("ProxyEnable");

                Log.Information("Current Settings");
                Log.Information($"ProxyEnable: {currentProxy}");
                Log.Information($"ProxyServer: {registry.GetValue("ProxyServer") ?? "<null>"}");

                bool proceed = Prompt.GetYesNo(
                    "Do you want to proceed?",
                    defaultAnswer: false,
                    promptColor: ConsoleColor.Black,
                    promptBgColor: ConsoleColor.White);

                if (proceed)
                {
                    switch (this.State)
                    {
                        case Status.On:
                            if (currentProxy == 1)
                            {
                                Log.Information("Your proxy is currently set to ON.");
                            }
                            else
                            {
                                registry.SetValue("ProxyEnable", 1);
                                registry.SetValue("ProxyServer", "example.com:8080");

                                if ((int)registry.GetValue("ProxyEnable", 0) == 0)
                                {
                                    Log.Information("Unable to enable the proxy.");
                                }

                                Log.Information("Your proxy has been turned ON.");
                            }

                            break;
                        case Status.Off:
                            if (currentProxy == 0)
                            {
                                Log.Information("Your proxy is currently set to OFF.");
                            }
                            else
                            {
                                registry.SetValue("ProxyEnable", 0);
                                registry.SetValue("ProxyServer", 0);

                                if ((int)registry.GetValue("ProxyEnable", 1) == 1)
                                {
                                    Log.Information("Unable to enable the proxy.");
                                }

                                Log.Information("Your proxy has been turned OFF.");
                            }

                            break;
                        default:
                            Log.Error("Unknown value");
                            break;
                    }
                }

                registry.Close();
            }
        }
    }
}
