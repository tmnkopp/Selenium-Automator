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
    [ValueSetterMeta(Selector = ".k-datepicker .k-input-inner")]
    public class KDatePickerValueSetter : BaseValueSetter, IValueSetter
    {
        private ChromeDriver driver;
        public void SetValue(SessionContext sessionContext, string ElementId)
        {
            this.driver = sessionContext.Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
             
            IWebElement Element = wait.Until(drv => drv.FindElement(By.CssSelector($"#{ElementId}"))); 
            IWebElement wrapper = wait.Until(drv => drv.FindElement(By.XPath($"//*[@id='{ElementId}']/..")));
            IWebElement button = wait.Until(drv => drv.FindElement(By.XPath($"//*[@id='{ElementId}']/../button")));

            ((IJavaScriptExecutor)driver).ExecuteScript(" window.scrollBy(0, -100); ");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", button);
            Thread.Sleep(100);
            IWebElement today = wait.Until(drv => drv.FindElement(By.XPath($"//*[contains(@class, 'k-today')]"))); 

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", today);
            
            var date = DateTime.Now.ToString("MM/dd/yyyy");
            Element.SendKeys("");
            Thread.Sleep(100);
            Element.SendKeys(date);  

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(.01);
            base.Log(sessionContext, ElementId);
            Thread.Sleep(500);
        }
    }
}