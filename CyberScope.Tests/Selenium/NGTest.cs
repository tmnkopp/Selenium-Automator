using Castle.Components.DictionaryAdapter;
using CyberScope.Automator;
using CyberScope.Automator.Providers;
using DocumentFormat.OpenXml.Bibliography;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;  
using OpenQA.Selenium.Support.UI; 
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Telerik.Web.Apoc;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using Xunit;
using static CyberScope.Automator.CsDriverService;

namespace CyberScope.Tests.Selenium
{
    public class NGTest 
    {
        #region CTOR  
        protected ILogger _logger;
        protected ITestOutputHelper output;
        public NGTest(ITestOutputHelper output)
        {
            this.output = output;
            var testInit = new TestInitializer()
                .InitLogging(output, (lc) => _logger = lc);
        }
        #endregion

        #region UNITTESTS   
        [Fact] 
        public void NextGenTest()
        { 
            using (var session = new NextGenSession(_logger))
            {
                session.Connect(); 

                var automator = new NaiveAutomator(session.Context);
                automator.OnPostAutomate += (sender, e) =>
                {
                    Thread.Sleep(5000);
                };
                try
                {
                    automator.Automate();
                }
                catch (Exception ex)
                { 
                    throw ex;
                } 
            }  
        }
        [Fact]
        public void CyberScopeTest()
        {
            using (var session = new CyberScopeSession(_logger))
            {
                session.Context.userContext = UserContext.Agency;
                session.Connect();
                session.ToTab("SAOP");

                // Add your test logic here
            }
        }

        #endregion
    }

    public class CyberScopeSession : BaseBrowserSession
    {
        public CyberScopeSession(ILogger logger) : base(logger)
        {
        }

        public override void Connect()
        {
            var url = SettingsProvider.appSettings[$"CSTargerUrl"];
            Driver.Navigate().GoToUrl(url); // Replace with your actual URL
            var connector = new DefaultCsConnector();
            connector.Connect(this.Context);
;        }
    }
    public class NextGenSession : BaseBrowserSession
    {
        public NextGenSession(ILogger logger) : base(logger) {
        }
       
        public override void Connect()
        {
            Driver.Navigate().GoToUrl("http://localhost:8081/"); // Replace with your actual URL
        }
    }
    public static class CyberScopeSessionExtensions
    {

        public static CyberScopeSession InitSections(this CyberScopeSession session, Func<DataCallSection, bool> SectionGroupPredicate)
        {
            IElementValueProvider oElementValueProvider = (IElementValueProvider)Activator.CreateInstance(session.getElementValueProviderType());
            var sectionsFiltered = session.Sections().Where(SectionGroupPredicate).ToList();
            foreach (DataCallSection section in sectionsFiltered)
            {
                session.ToSection(section);

                var controls = session.PageControlCollection().EmptyIfNull();

                foreach (IAutomator control in controls)
                {
                    ((IAutomator)control).Automate();
                    session.Context.RefreshDefaults(oElementValueProvider);
                }
                 
            }
            return session;
        }
        public static BaseBrowserSession ToSection(this CyberScopeSession session, DataCallSection Section)
        {
            SelectElement se = new SelectElement(session.Driver.FindElement(By.CssSelector("*[id*='_ddl_Sections']")));
            se?.Options.Where(o => o.Text.Contains(Section?.SectionText)).FirstOrDefault()?.Click();
            return session;
        }
        public static BaseBrowserSession ToSection(this CyberScopeSession session, Func<DataCallSection, bool> Predicate)
        {
            var section = session.Sections().Where(Predicate).FirstOrDefault();
            session.ToSection(section);
            return session;
        }
        public static BaseBrowserSession ToSection(this CyberScopeSession session, int Index)
        {
            var driver = session.Driver;
            SelectElement se = new SelectElement(driver.FindElement(By.CssSelector("*[id*='_ddl_Sections']")));
            if (Index < 0)
                Index = se.Options.Count() - 1;
            se.SelectByIndex(Index);
            return session;
        }
        public static IEnumerable<DataCallSection> Sections(this CyberScopeSession session)
        {
            var driver = session.Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            IReadOnlyCollection<IWebElement> ele;
            ele = wait.Until(drv => drv.FindElements(By.XPath($"//*[contains(@id, '_ddl_Sections')]/option")));
            var groups = (from e in ele
                          select new DataCallSection
                          {
                              URL = e.GetAttribute("value"),
                              SectionText = e.Text
                          }).ToList();
            return groups;
        }
        public static CyberScopeSession ToUrl(this CyberScopeSession session, string url)
        {
            var @base = SettingsProvider.appSettings[$"CSTargerUrl"];
            url = url.Replace("~", @base);
            session.Driver.Navigate().GoToUrl(url);
            return session;
        }
        public static CyberScopeSession ToTab(this CyberScopeSession session, string TabText, bool Launch = true, string FormName = "")
        {
            var driver = session.Driver;
            IWebElement ele;
            WebDriverWait wait;
            if (!driver.Url.Contains("ReporterHome.aspx"))
            {
                session.ToUrl("~ReporterHome.aspx");
            }
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            var eles = wait.Until(drv => drv.FindElements(By.XPath($"//*[contains(@id, '_Surveys')]//*[contains(@class, 'rtsTxt')]")))?.Reverse();
            ele = (from e in eles where Regex.IsMatch(e.Text, TabText) || e.Text.Contains(TabText) select e).FirstOrDefault();
            ele?.Click();
            Thread.Sleep(1000);

            string FormLauncherXPATH = SettingsProvider.appSettings["FormLauncherXPATH"];
            if (SettingsProvider.TestSettings["DataCall"].ContainsKey("FormLauncherXPATH"))
            {
                FormLauncherXPATH = SettingsProvider.TestSettings["DataCall"]["FormLauncherXPATH"];
            }
            else if (session.ElementExists(By.XPath("//li[contains(@class, 'rtsSelected')]//span[contains(text(), '18-02')]")))
            {
                FormLauncherXPATH = SettingsProvider.appSettings["HVAFormLauncherXPATH"];
            }

            if (session.ElementExists(By.XPath(FormLauncherXPATH)))
            {
                ele = wait.Until(d => {
                    return d.FindElement(By.XPath(FormLauncherXPATH));
                });
                ele = wait.Until(drv => drv.FindElement(By.XPath(FormLauncherXPATH)));
                ele?.Click();
                return session;
            }
            var FormExpanderXPATH = SettingsProvider.appSettings[$"FormExpanderXPATH"];
            if (session.ElementExists(By.XPath(FormExpanderXPATH)))
            {
                var elm = driver.FindElements(By.XPath(FormExpanderXPATH));
                elm[0].Click();
                try
                {
                    if (string.IsNullOrEmpty(FormName))
                    {
                        FormName =  SettingsProvider.TestSettings["DataCall"]["Form"];
                    }
                }
                catch (Exception ex)
                {
                    session.Context.Logger.Error("{@Exception}", new { ex.Message });
                }
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                var element = driver.FindElement(By.XPath($"//tr[contains(@id, 'formsgrid')]//tr/td[contains(text(), '" + FormName + "')]/../td/a"));
                element.Click();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.0001);
                return session;
            } 
            return session; 
        }
        public static CyberScopeSession FismaFormEnable(this CyberScopeSession session)
        {
            string btntext = "CBButtPanel1_btnEdit";
            WebDriverWait wait = new WebDriverWait(session.Driver, TimeSpan.FromSeconds(1));
            var ele = wait.Until(drv => drv.FindElement(By.XPath($"//*[contains(@id, '{btntext}')]")));
            ((IJavaScriptExecutor)session.Driver).ExecuteScript("arguments[0].click();", ele);
            return session;
        }

        public static CyberScopeSession FismaFormSave(this CyberScopeSession session)
        {
            string btntext = "_btnSave";
            WebDriverWait wait = new WebDriverWait(session.Driver, TimeSpan.FromSeconds(1));
            var ele = wait.Until(drv => drv.FindElement(By.XPath($"//*[contains(@id, '{btntext}')]")));
            ((IJavaScriptExecutor)session.Driver).ExecuteScript("arguments[0].click();", ele);
            return session;
        }
        public static bool FismaFormValidates(this CyberScopeSession session)
        {
            session.ToSection(-1);
            var success = new WebDriverWait(session.Driver, TimeSpan.FromSeconds(5))
            .Until(dvr => dvr.FindElements(By.CssSelector("#ctl00_ContentPlaceHolder1_lblSuccessInfo")));
            if (success.Count() > 0)
            {
                return success[0].Text.Contains("Your form has been validated and contains no errors.");
            }
            session.Context.Logger.Warning($"FismaForm InValid");
            return false;
        }

    }
    public static class NextGenSessionExtensions
    { 
        public static NextGenSession Connect(this NextGenSession session)
        {
            session.Driver.Navigate().GoToUrl("http://localhost:8081/"); // Replace with your actual URL
            return session;
        } 
    }
    public static class BaseBrowserSessionExtensions
    {
        public static Type getElementValueProviderType(this CyberScopeSession session)
        {
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            var ElementValueProviders = (from assm in assms
                                         from t in assm.GetTypes()
                                         where typeof(IElementValueProvider).IsAssignableFrom(t) && t.IsClass
                                         select t).ToList();

            Type answerProvider = typeof(ElementValueProvider);
            ElementValueProviders.ForEach(t => {
                var attr = t.GetCustomAttribute<ElementValueProviderMeta>(false);
                if (!string.IsNullOrEmpty(attr?.XpathMatch))
                {
                    //var e = session.GetElement(By.XPath(attr.XpathMatch));
                    WebDriverWait wait = new WebDriverWait(session.Driver, TimeSpan.FromSeconds(1));
                    var element = (from e in wait.Until(dvr => dvr.FindElements(By.XPath(attr.XpathMatch)))
                                   where e.Enabled && e.Displayed
                                   select e).FirstOrDefault();
                    if (element != null) answerProvider = t;
                }
            });
            return answerProvider;
        }
        public static IEnumerable<IAutomator> PageControlCollection(this CyberScopeSession session)
        {
            var automators = new List<IAutomator>();
            var driver = session.Driver;
            var controlLocators = SettingsProvider.ControlLocators.EmptyIfNull();

            foreach (ControlLocator controlLocator in controlLocators)
            {
                bool isExcluded = false;
                foreach (var exclude in controlLocator.Exclude)
                {
                    isExcluded = (from e in driver.FindElements(By.XPath($"{exclude}")) select e).FirstOrDefault() != null;
                    if (isExcluded) continue;
                }
                if (isExcluded) continue;

                var eles = (from e in driver.FindElements(By.XPath($"{controlLocator.Locator}"))
                            where e.Displayed == true && e.Enabled == true
                            select e).ToList();
                if (eles.Count > 0)
                {
                    var type = Assm.GetTypes().Where(t => t.Name == controlLocator.Type).FirstOrDefault();
                    var exists = (from a in automators where a.GetType().Name == type.Name select a).ToList().Count > 0;
                    if (exists) continue;

                    string ValueSetterType = (!string.IsNullOrWhiteSpace(controlLocator.ValueSetterTypes)) ? controlLocator.ValueSetterTypes : ".*";

                    // Constructor injection of SessionContext
                    IAutomator obj = (IAutomator)Activator.CreateInstance(Type.GetType($"{type.FullName}"), session);
                    obj.ContainerSelector = $" #{session.GetElementIDByXpath(controlLocator.Selector)} ";
                    obj.Overwrite = controlLocator.Overwrite;
                    automators.Add(obj);
                    session.Context.Logger.Information("{@controlLocator}", new { controlLocator });
                }
            }
            return automators;
        }
        public static string GetElementIDByXpath(this BaseBrowserSession session, string XPathSelector)
        { 
            var elements = session.Driver.FindElements(By.XPath(XPathSelector));
            if (elements.Count > 0)
                return elements[0].GetAttribute("id");
            return null;
        } 
        public static bool ElementExists(this BaseBrowserSession session, By by)
        {
            try
            {
                var elements = session.Driver.FindElements(by);
                return elements.Count > 0;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
    
    public abstract class BaseBrowserSession : IDisposable
    {
        private readonly Lazy<ChromeDriver> _driver;
        private Lazy<SessionContext> _context;
        private bool _isDisposed;  
        public BaseBrowserSession(ILogger logger)
        { 
            ChromeDriverFactory factory = new ChromeDriverFactory(); 
            _driver = new Lazy<ChromeDriver>(() => factory.CreateDriver());
            _context = new Lazy<SessionContext>(() =>
            {
                var ctx = ContextFactory.Create(_driver.Value, logger);
                ctx.RefreshDefaults();
                return ctx;
            });


        } 
        public ChromeDriver Driver => _driver.Value;
        public SessionContext Context => _context.Value;
        public abstract void Connect(); // Abstract method to be implemented by derived classes 
        // Cleans up the browser process when this specific session is closed
        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_driver.IsValueCreated)
                {
                    _driver.Value.Quit();
                    _driver.Value.Dispose();
                }
                _isDisposed = true;
            }
        }
    } 
    public class ChromeDriverFactory
    {
        public ChromeDriver CreateDriver()
        {
            string driverPath = ResolveDriverPath();
            ChromeOptions options = ConfigureOptions();
            var service = CreateService(driverPath);

            var driver = new ChromeDriver(service, options);
            ApplyWindowSettings(driver, options);

            driver.Manage().Cookies.DeleteCookieNamed("_selenium");
            var cookie = new Cookie("_selenium", DateTime.Today.ToString());
            driver.Manage().Cookies.AddCookie(cookie);
            return driver;
        }

        private string ResolveDriverPath()
        {
            var managedPath = new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
            return managedPath.Replace("chromedriver.exe", "" );
        }

        private ChromeOptions ConfigureOptions()
        {
            var options = new ChromeOptions();
            var args = CyberScope.Automator.SettingsProvider.ChromeOptions.EmptyIfNull(); 
            foreach (var arg in args)
            {
                options.AddArgument(arg);
            }
            return options;
        }

        private ChromeDriverService CreateService(string driverPath)
        {
            var service = ChromeDriverService.CreateDefaultService(driverPath);
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;
            return service;
        }

        private void ApplyWindowSettings(ChromeDriver driver, ChromeOptions options)
        {
            // Check the source collection directly instead of serializing to JSON
            var args = CyberScope.Automator.SettingsProvider.ChromeOptions.EmptyIfNull().ToList(); 
            if (args.Contains("minimize")) driver.Manage().Window.Minimize();
            if (args.Contains("maximize")) driver.Manage().Window.Maximize();
        }
    } 
}

