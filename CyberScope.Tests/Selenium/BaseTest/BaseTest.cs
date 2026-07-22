using Serilog;
using Xunit;
using CyberScope.Automator;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


namespace CyberScope.Tests.Selenium
{
    public abstract class BaseTest{

        #region FIELDS 
        protected ILogger _logger;
        protected ITestOutputHelper output;
        protected CsDriverService ds;
        protected Dictionary<string,string> config;
        #endregion

        #region CTOR
        public BaseTest(ITestOutputHelper output)
        {
            this.output = output;
            var testInit = new TestInitializer() 
                .InitLogging(output, (lc) => _logger = lc);
  
            ds = new CsDriverService(_logger);
            config = CyberScope.Automator.SettingsProvider.TestSettings["DataCall"];
        }
        #endregion

        #region PROPS
        public string DataDir { 
            get => Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin.*", "\\Data\\"); 
         }
        #endregion

        #region METHODS

        #endregion
    }
}
