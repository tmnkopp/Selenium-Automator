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
    public class MFA : BaseTest
    {
        #region FIELDS  
        IWebElement ele;
        IReadOnlyCollection<IWebElement> eles;
        WebDriverWait wait;
        string Tab = "MFA";
        ChromeDriver driver;
        SessionContext context;
        SprocExecutor db = new SprocExecutor();
        #endregion

        #region CTOR 
        public MFA(ITestOutputHelper output) : base(output)
        { 
            db.Params.Add("PK_OrgSubmission", "148721");
            db.Params.Add("UserId", "1838"); 
            db.Params.Add("MODE", "RepSec"); 
        }
        #endregion

        #region UNITTESTS     
        [Fact]
        public void MFA_FormValidator_Validates()
        { 
            this.InitAgency(); 
            string path = this.config.ContainsKey("invalid") ? this.config["invalid"] : Utils.GetDataPath() + "MFA_INVALID.xlsx"; 
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))).SendKeys(path);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]"))).Click();
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
            var errors = wait.Until(d => d.FindElements(By.XPath("//*[contains(@id, '_bl_Errors')]/li"))).Count() > 0;
            Assert.True(errors);

        }
        [Fact]
        public void MFA_Importer_Imports()
        {
            this.InitAgency();
             
            string path = this.config.ContainsKey("valid") ? this.config["valid"] : Utils.GetDataPath() + "MFA.xlsx"; 
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))).SendKeys(path);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]"))).Click(); 
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete"); 
            int rows = wait.Until(d => d.FindElements(By.XPath("//a[contains(@id, '_EditButton')]"))).Count(); 
            Assert.True(rows  > 0);
 
            //var result = db.Execute("MFAEncryption_CRUD").Rows[0]["OrgSub_Status"].ToString();
            //Assert.Contains("IP", result);

            //var validates = ds.FismaFormValidates();
            //Assert.True(validates); 
            //if (validates) _logger.Information("FismaFormValidated {o}", new { Tab });
        }
        [Fact]
        public void MFA_AddNewRecord_Adds()
        {
            this.InitAgency();

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_AddNewRecordButton')]"))).Click(); 
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
            context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };

            var na = new CSNaiveAutomator(context);
            na.ContainerSelector = ".rgEditRow";
            na.Automate();

            var element = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_PerformInsertButton')]")));  
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(1000);
            element.Click();
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");

            int rows = wait.Until(d => d.FindElements(By.XPath("//a[contains(@id, '_EditButton')]"))).Count();
            Assert.True(rows > 0);
            var errors = wait.Until(d => d.FindElements(By.XPath("//*[contains(@id, '_bl_Errors')]/li"))).Count() < 1;
            Assert.True(errors);

        }

        private void InitAdmin()
        { 
            driver = ds.CsConnect(UserContext.Admin).Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ds.ToTab(this.Tab);
        }
        private void InitAgency()
        { 
            driver = ds.CsConnect(UserContext.Agency).Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ds.ToTab(this.Tab);
        }
        #endregion
    }
}
