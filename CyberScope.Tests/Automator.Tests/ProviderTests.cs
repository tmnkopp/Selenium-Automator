using CyberBalance.CS.Core;
using CyberBalance.CS.Web.UI;
using CyberScope.Automator;
using CyberScope.Selenium;
using DocumentFormat.OpenXml.Vml;
using Microsoft.Win32;
using NCalc.Domain;
using Newtonsoft.Json.Converters;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;
using Serilog;
using Serilog.Events;
 
using System;
using System.Collections.Generic; 
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions; 
using System.Threading;
using System.Threading.Tasks;
using Telerik.Web.Apoc;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using Xunit; 
namespace Automator.Tests
{
    public class ProviderTests
    {
        ILogger _logger;
        private readonly ITestOutputHelper output;
        public ProviderTests(ITestOutputHelper output)
        {
            var LoggerDir = SettingsProvider.appSettings[$"LoggerDir"];
            this.output = output;
            _logger = new LoggerConfiguration()
            .WriteTo.Sink(new SimpleXunitSink(output))
            .WriteTo.File($"{LoggerDir}\\log.log",
                    LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
            .CreateLogger();
        } 
        [Fact]
        public void DefaultValueProvider_Provides()
        {
            var d = new Dictionary<string, string>();
            d.Add("{1.1.5}", "{1.1.6} + {1.1.6}"); 
            d.Add("{1.1.6}", "{1.1.5} - {1.1.5}");

            var a = new Dictionary<string, string>();
            a.Add("{1.1.6}", "6");
            a.Add("{1.1.5}", "5");
            a.Add("{1.1.4}", "4");

            foreach (var key in d.Keys.ToArray())
            {
                var expressions = Regex.Split(d[key], @"\s*[\+|\-]\s*");
                foreach (var expression in expressions) 
                    if(a.ContainsKey(expression)) 
                        d[key] = d[key].Replace(expression, a[expression]); 
            }
            var m = d;
        }
        [Fact]
        public void TOMLProvider_Provides()
        {
            var inputs = SettingsProvider.InputDefaults;
            foreach (var item in inputs)
            {
                var k = item.Key;
                var v = item.Value;
            }
        } 
        [Fact]
        public void Location_Parser_Parses()
        {
            string city, state, zip;
            string location = "San Buenaventura (Ventura), MI 12340";
            var matches = Regex.Match(location.Trim(), @"^(.+),\s+(\w{2})\s+(\d{5})$");
            if(matches.Success)
            {
                if(matches.Groups.Count == 4)
                {
                    city = matches.Groups[1].Value.Trim();
                    state = matches.Groups[2].Value.Trim();
                    zip = matches.Groups[3].Value.Trim();
                }
            }
           


        }
        [Fact]
        public void ChromeDriver_Provider_Provides()
        {
            new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            chromeDriverService.SuppressInitialDiagnosticInformation = true;

            IWebDriver _driver = new ChromeDriver(chromeDriverService);
            Assert.NotNull(_driver);
        }
    }   
}
