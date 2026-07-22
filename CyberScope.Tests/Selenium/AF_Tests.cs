using CyberScope.Automator;
using CyberScope.Selenium;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;


namespace CyberScope.Tests.Selenium
{
    public class AF_Tests : BaseTest
    {
        #region CTOR 
        public AF_Tests(ITestOutputHelper output) : base(output)
        {
        }
        #endregion

        #region UNIT TESTS 
        //  https://www.automatetheplanet.com/wp-content/uploads/2017/02/MostCompleteWebDriverCSharpCheetSheet.pdf
        //  https://devhints.io/xpath    |     http://xpather.com/
        
        [Fact] // MUST INCLUDED FOR TEST
        public void SCUBA_Form_Saves()
        {
            var driver = ds.CsConnect(UserContext.Agency).Driver;
            ds.ToTab("Cloud Services");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
             
            var addnew_element = driver.FindElement(By.XPath("//span[contains(@id, 'AddNewRecordButton')]"));
            addnew_element.Click();

            Thread.Sleep(1000);

            var context = new SessionContext(this._logger, driver);
            context.Defaults = new DefaultInputProvider(driver).DefaultValues;
            
            var na = new CSNaiveAutomator(context);
            na.ContainerSelector = ".rgEditRow";
            na.Automate();
             
            var save_button = wait.Until(d => d.FindElement(By.XPath("//a[contains(@id, 'PerformInsertButton')]"))); 
            save_button.Click();

            var rows = wait.Until(d => d.FindElements(By.XPath("//a[contains(@id, '_EditButton')]")));
             
        }
        #endregion
    }
}
