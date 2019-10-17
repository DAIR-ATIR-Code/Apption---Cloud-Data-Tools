
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using ElectronNET.API;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog.Layouts;
using WebAppMaterialize.App;

namespace WebAppMaterialize.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
			SetupLogConfiguration();
            BuildWebHost(args).Run();
        }

		private static void SetupLogConfiguration()
		{
			var config = new NLog.Config.LoggingConfiguration();
			var filename = $"DataToolsLog-{DateTime.Today.ToShortDateString()}.txt";
			var logfile = new NLog.Targets.FileTarget("logfile") { FileName = filename };
			var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            var jsTarget = new NLogJSInteropTarget();
            jsTarget.Layout = @"${uppercase:${level}} - ${message} ${exception:format=tostring}"; ;
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, jsTarget);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
			config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);

			NLog.LogManager.Configuration = config;
		}

		public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = null;
                })
                //.UseElectron(args)
                .UseStartup<Startup>()
                .Build();
    }
}

