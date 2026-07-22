using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic; 
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace CyberScope.Tests.Selenium
{
    public class TestInitializer
    {
        public TestInitializer()
        {
            
        }
        public TestInitializer InitLogging(ITestOutputHelper output, Action<ILogger> logConfig)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                     ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                     ?? "Development";


            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();

            var LoggerDir = CyberScope.Automator.SettingsProvider.appSettings[$"LoggerDir"];
            ILogger _LoggerConfig; 
            if (!string.IsNullOrWhiteSpace(LoggerDir))
            {
                if (Directory.Exists(LoggerDir))
                {
                    _LoggerConfig = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                    .WriteTo.Sink(new SimpleXunitSink(output))
                    .WriteTo.File($"{LoggerDir}\\log.log",
                        LogEventLevel.Verbose,
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true)
                    .CreateLogger();

                    logConfig(_LoggerConfig);
                    return this;
                }
                else{ 
                    Console.WriteLine($"Logger directory '{LoggerDir}' does not exist.");
                }

            }
            else
            {
                _LoggerConfig = new LoggerConfiguration().MinimumLevel.Verbose()
                .WriteTo.Sink(new SimpleXunitSink(output))
                .CreateLogger();
                logConfig(_LoggerConfig);
                return this;
            }
            return this;
        } 
    }
}
