using AngleSharp.Dom;
using Microsoft.Extensions.FileSystemGlobbing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CyberScope.Automator
{
    [ValueSetterMeta(Selector = ".k-dropdownlist .k-input-inner")]
    public class KDropDownListValueSetter : BaseValueSetter, IValueSetter
    {
        private ChromeDriver driver;
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            this.driver = sessionContext.Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
            double waitDefault = 2;
            if (SettingsProvider.appSettings.ContainsKey("RadDropDownListValueSetterWait"))
            {
                waitDefault = Convert.ToDouble(SettingsProvider.appSettings["RadDropDownListValueSetterWait"] ?? "2");
            }
            ((IJavaScriptExecutor)this.driver).ExecuteScript(@"
                document.getElementsByTagName('body')[0].style.height='500vh';
            ");
            var element = driver.FindElement(By.Id(ElementId));
            var script = "arguments[0].scrollIntoView(true);";
            ((IJavaScriptExecutor)driver).ExecuteScript(script, element);

            Thread.Sleep(250); 
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"*[aria-describedby='{ElementId}']")));
            var elementName = Element.GetAttribute("aria-controls");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", Element);
         

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(waitDefault);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitDefault));
            Thread.Sleep(800);
            inputs = wait.Until(drv => drv.FindElements(By.CssSelector($".k-list-scroller #{elementName} li")));
            inputs = (from i in inputs where  i.Displayed   select i).ToList();

            bool matched = false;
            foreach (var item in sessionContext.Defaults.EmptyIfNull())
            { 
                if (Regex.Match(elementName, item.Key, RegexOptions.IgnoreCase).Success)
                { 
                    inputs[Convert.ToInt16(item.Value)].Click();
                    matched = true;
                    break;
                } 
            }
            if (!matched)
            {
                if (inputs.Count >= 3) inputs[2].Click();
                else if (inputs.Count >= 2) inputs[1].Click();
                else inputs[0].Click();
            } 
     
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
            base.Log(sessionContext, ElementId);
            Thread.Sleep(Convert.ToInt32(waitDefault) * 500);
        }
    }
}