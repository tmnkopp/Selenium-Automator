using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CyberScope.Automator 
{

    public class NaiveAutomator : ControlAutomator 
    {
        #region PROPS  
        private Random _random = new Random();
        #endregion

        #region CTOR
        public NaiveAutomator(SessionContext sessionContext) : base(sessionContext)
        {
        } 
        #endregion

        #region METHODS
  
        public override void Automate()
        { 
            var args = new AutomatorEventArgs(sessionContext);
            PreAutomate(args);
            int posts = 0,
                postcnt = 0,
                precnt = 0,
                valuesSet = 0;

            this.applyPreDelay();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.1);
            while (true)
            {

                precnt = GetDisplayedElementsCount();
                ((IJavaScriptExecutor)driver).ExecuteScript("document.title=arguments[0];", $"{precnt}:{postcnt}:{valuesSet}");

                foreach (var setter in ValueSetters)
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);

                    var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(setter.GetType(), typeof(ValueSetterMeta));
                    var selector = $"{this.ContainerSelector} {meta.Selector}";


                    //if (driver.FindElements(By.CssSelector($"{selector}")).Count < 1)
                    //    continue; 

                    var elementHasMatch = this.matchElement(setter);
                    if (!elementHasMatch)
                        continue;

                    this.ElementIdIterator(selector, (ElementId) =>
                    {
                        //driver.FindElement(By.TagName($"head"))?.Click(); 
                        new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until((dvr) =>
                        {
                            return ((IJavaScriptExecutor)dvr).ExecuteScript("return document.readyState").Equals("complete");
                        });

                        sessionContext.Logger.Verbose($"NaiveAutomator:ValueSetterMeta {selector} {ElementId}");

                        var element = driver.FindElement(By.Id(ElementId));
                        var script = "arguments[0].scrollIntoView(true);";
                        ((IJavaScriptExecutor)driver).ExecuteScript(script, element);
                        //Thread.Sleep(50);
                        IValueSetter valueSetter = setter;
                        valueSetter.Overwrite = posts == 0;
                        try
                        {
                            valueSetter.SetValue(sessionContext, ElementId);
                            valuesSet++;
                        }
                        catch (StaleElementReferenceException ex)
                        {
                            sessionContext.Logger.Warning($"NaiveAutomator StaleElementReferenceException {ElementId} {ex.Message} {ex.InnerException}");
                        }
                        catch (Exception ex)
                        {
                            sessionContext.Logger.Error($"NaiveAutomator Exception {ElementId} {ex.Message} {ex.InnerException}");
                            if (!Regex.IsMatch(ex.Message, "interactable|invalid element"))
                                throw new Exception($"NaiveAutomator {ElementId} {ex.Message} {ex.InnerException}");
                        }
                    });

                }
                postcnt = GetDisplayedElementsCount();
                ((IJavaScriptExecutor)driver).ExecuteScript("document.title=arguments[0];", $"{precnt}:{postcnt}:{valuesSet}");
                sessionContext.Logger.Information($"NaiveAutomator precnt:{precnt}  postcnt:{postcnt} valuesSet:{valuesSet}");
                posts++;
                if (precnt >= postcnt || posts > 8)
                    break;
            }
            PostAutomate(args);
 
        }
        #region PRIV
        private bool matchElement(IValueSetter setter)
        {
            var meta = (ValueSetterMeta)Attribute.GetCustomAttribute(setter.GetType(), typeof(ValueSetterMeta));
            var selector = $"{this.ContainerSelector} {meta.Selector}";
            var exclude = meta.Exclude?.Split(',');

            if (exclude != null)
            {
                foreach (var css in exclude)
                {
                    if (driver.FindElements(By.CssSelector($"{this.ContainerSelector} {css}")).Count > 0)
                        return false;
                }
            }
            if (driver.FindElements(By.CssSelector($"{selector}")).Count > 0)
                return true;
            return false;
        }
        private void applyPreDelay()
        {
            if (SettingsProvider.appSettings.ContainsKey("NaiveAutomatorPreDelayPattern"))
            {
                var pattern = driver.FindElements(By.XPath(SettingsProvider.appSettings["NaiveAutomatorPreDelayPattern"]))?.Count > 0;
                if (pattern)
                {
                    Thread.Sleep(1200);
                    this.sessionContext.Logger.Information("{type}.{method} {log}"
                    , this.GetType().Name
                    , MethodBase.GetCurrentMethod().Name
                    , SettingsProvider.appSettings["NaiveAutomatorPreDelayPattern"]);
                }
            }
        }
        private void ElementIdIterator(string Selector, Action<string> InputAction)
        {
            inputs = driver.FindElements(By.CssSelector($"{Selector}"));
            var elmts = (from i in inputs
                         where i.Enabled && i.Displayed
                         let elementId = i.GetAttribute("id") // Extract the ID once
                         where !string.IsNullOrWhiteSpace(elementId) // Filter out missing, null, or blank IDs
                         select new { id = elementId }).ToList();
            while (elmts.Count > 0)
            {
                InputAction(elmts[0].id);
                elmts.RemoveAt(0);
            }
        }
        private int GetDisplayedElementsCount()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            IWebElement container = wait.Until(d => d.FindElement(By.CssSelector($"{this.ContainerSelector}")));

            IReadOnlyCollection<IWebElement> eCollection;
            try
            {
                eCollection =
                (from e in container.FindElements(By.XPath($"//input|//select|//textarea"))
                 where e.Displayed && e.Enabled
                 select e).ToList();
                return eCollection.Count;
            }
            catch (Exception ex)
            {
                sessionContext.Logger.Error($"NaiveAutomator {ex}");
            }
            return 0;
        }
        #endregion
        #endregion
    }
}
