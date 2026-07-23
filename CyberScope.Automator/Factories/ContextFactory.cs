using Serilog;
using CyberScope.Automator;
using OpenQA.Selenium.Chrome;
using DocumentFormat.OpenXml.Bibliography;
using Serilog.Core;

namespace CyberScope.Automator
{
    public static class ContextFactory{ 
        public static SessionContext Create(ChromeDriver driver, Serilog.ILogger logger)
        {
            return new SessionContext(logger, driver)
            {
                Driver = driver,
                Logger = logger,
                Defaults = new DefaultInputProvider(driver).DefaultValues
            }; 
         } 
    } 
}

