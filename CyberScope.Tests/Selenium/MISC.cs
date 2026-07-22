using AngleSharp.Dom; 
using CyberScope.Automator;
using CyberScope.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit;
using System.IO;
using Org.BouncyCastle.Crypto;
using OpenQA.Selenium.Remote;

namespace CyberScope.Tests.Selenium
{

    
    public class MISC : BaseTest
    {
        #region FIELDS  
        IWebElement ele;
        IReadOnlyCollection<IWebElement> eles;
        WebDriverWait wait;
        string Tab = "22-01";
        ChromeDriver driver;
        SprocExecutor db = new SprocExecutor();
        #endregion

        #region CTOR 
        public MISC(ITestOutputHelper output) : base(output)
        {
            db.Params.Add("PK_OrgSubmission", "148200");
            db.Params.Add("UserId", "1838");
            db.Params.Add("UserMode", "Admin");
            db.Params.Add("MODE", "SELECT"); 
        }
        #endregion

        #region UNITTESTS   
        Func<IWebDriver, bool> readyStateComplete = (IWebDriver d) => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete");

        [Fact]
        public void BOD2602_Record__Adds()
        {
            //var testInit = new TestInitializer();
            //testInit.InitIIS();
             
            // this.config = SettingsProvider.TestSettings["FEDRAMP_CLOUD"];
            driver = ds.CsConnect(UserContext.Agency).Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ds.ToTab("26-02");
            ds.ToSection(s => s.SectionText.ToUpper().Contains("2"));

            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_AddNewRecordButton')]"))).Click();

            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues 
            };
            
            var na = new CSNaiveAutomator(context);
            na.ContainerSelector = "div[id$='_MainGrid']";
            na.Automate();

            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo({  left: 0 });");
            Thread.Sleep(1000);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_PerformInsertButton')]"))).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            var elements = wait.Until(d => d.FindElements(By.XPath("//a[contains(@id, '_EditButton')]")));
            Assert.True(elements.Count() > 0);
            ds.DisposeDriverService();

        }
        [Fact]
        public void BOD2602_VALID_UPLOAD()
        {
            //var testInit = new TestInitializer();
            //testInit.InitIIS();

            this.config = SettingsProvider.TestSettings["BOD2602"];
            driver = ds.CsConnect(UserContext.Agency).Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ds.ToTab("26-02");
            ds.ToSection(s => s.SectionText.ToUpper().Contains("2"));

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'Uploader_fileUpload')]"))).SendKeys(this.config["BOD2602_VALID"]);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'CBFileUploader_cmdUpload')]"))).Click();
            ds.DisposeDriverService();

        }
        [Fact]
        public void BOD2501_Record__Adds()
        { 
            // this.config = SettingsProvider.TestSettings["FEDRAMP_CLOUD"];
            driver = ds.CsConnect(UserContext.Agency).Driver; 
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ds.ToTab("25-01").ToSection(s => s.SectionText.ToUpper().Contains("1"));

            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
 
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_AddNewRecordButton')]"))).Click();

            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues

            };
            var na = new CSNaiveAutomator(context); 
            na.ContainerSelector = "div[id$='_MainGrid']";
            na.Automate();

            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo({  left: 0 });");
            Thread.Sleep(1000);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_PerformInsertButton')]"))).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            var elements = wait.Until(d => d.FindElements(By.XPath("//a[contains(@id, '_EditButton')]")));
            Assert.True(elements.Count() > 0);
            ds.DisposeDriverService();

        }
        [Fact]
        public void CloudGrid()
        {
         
            this.config = SettingsProvider.TestSettings["FEDRAMP_CLOUD"];
            driver = ds.CsConnect(UserContext.Agency).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ds.ToTab("CIO").ToSection(s => s.SectionText.ToUpper().Contains("CLOUD"));

            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
 
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display='none'",
                wait.Until(d => d.FindElement(By.XPath("*//nav")))
            );
            
            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until((d) =>
            {
                return ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete");
            });
            if (File.Exists(this.config["Agency_Valid"])) File.Delete(this.config["Agency_Valid"]);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_rbtnDownload')]"))).Click();
            new WebDriverWait(driver, TimeSpan.FromSeconds(9)).Until((d) =>
            {
                return ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete");
            });
            Thread.Sleep(1000); 
            try
            {  
                wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'Uploader_fileUpload')]"))).SendKeys(this.config["Agency_Valid"]);
                wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'CBFileUploader_cmdUpload')]"))).Click();
            }
            finally
            {
                if (File.Exists(this.config["Agency_Valid"])) File.Delete(this.config["Agency_Valid"]); 
            } 
        }
 
        [Fact]
        public void AllTabsOpen(){
            var driver = ds.CsConnect(UserContext.Agency).Driver;  
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            var tabs = wait.Until(drv => drv.FindElements(By.XPath($"//*[contains(@id, '_Surveys')]//*[contains(@class, 'rtsTxt')]")))?.Reverse();
            foreach(IWebElement tab in tabs)
            { 
                if(tab.Text.Trim() != "History")
                {
                    ds.ToTab(tab.Text.Trim());
                    //ds.OpenTab
                    Thread.Sleep(2000);
                } 
            }   
        }
 
        [Fact]
        public void BOD2602_Admin(){
            ///
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver);
            context.RefreshDefaults();
            var def = context.Defaults;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(9));
            ds.ToUrl("~/Maintenance/BOD2602_Admin.aspx");
        }

        [Fact]
        public void FedRAMP_Admin_Uploads()
        {
            this.config = SettingsProvider.TestSettings["FEDRAMP_CLOUD"];
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(9));
            ds.ToUrl("~/Maintenance/FedRAMP_Admin.aspx");

            wait.Until(readyStateComplete);
             
            wait.Until(d => d.FindElement(By.XPath("//span[contains(text(), 'Admin Upload')]"))).Click();
             
            string path = this.config["Prepop"];
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'AdminFileUploader_fileUpload')]"))).SendKeys(path);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'AdminFileUploader_cmdUpload')]"))).Click();
        
            //wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'CBFileUploader_cmdUpload')]"))).Click();
            //driver.Quit();
        }


        [Fact]
        public void BOD2302_Admin_Init()
        {
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ds.ToUrl("~/Maintenance/BOD_2302_Admin.aspx");
        }
        [Fact]
        public void BOD2501_Artifact__Uploads()
        {
           
            driver = ds.CsConnect(UserContext.Agency).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            ds.ToTab("25-01");
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(9));
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'GECBtnExpandColumn')]"))).Click(); 
            // wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))).SendKeys(this.config["yaml_invalid"]);
            //wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'ddlSubmissionType_Input')]"))).Click();
           // wait.Until(d => d.FindElement(By.XPath("//div[@class='rcbSlide']//li[2]"))).Click();
  
            //yaml_valid 
        }
        [Fact]
        public void BOD2302_Agency_Init()
        {
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            ds.ToTab("23-02");
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
        }
        [Fact]
        public void BOD2302_Admin_Import()
        {  
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Admin).Driver; 
            var context = new SessionContext(this._logger, driver)
            { 
                Defaults = new DefaultInputProvider(driver).DefaultValues
            }; 
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2)); 
            ds.ToUrl("~/Maintenance/BOD_2302_Admin.aspx");
             
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_btnArchiveAll')]"))).Click(); 
            driver.SwitchTo().Alert().Accept(); 
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))).SendKeys(this.config["valid"]);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]"))).Click(); 
            
            Assert.Contains("Admin", db.Execute("BOD2302_CRUD").Rows[0]["ResolveStatus_Display"].ToString()); 
         
        }
        [Fact]
        public void BOD1902_Admin_Import()
        {
            this.config = SettingsProvider.TestSettings["BOD1902"];
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_lnkManageAgencyRemediation')]"))).Click();


            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_btnArchiveAll')]"))).Click();
            driver.SwitchTo().Alert().Accept();

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_fileupload')]"))).SendKeys(this.config["valid"]);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_btnUpload')]"))).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'btnIssueToAgency')]"))).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");

        }

        [Fact]
        public void BOD2302_Admin_IssueAll()
        {
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ds.ToUrl("~/Maintenance/BOD_2302_Admin.aspx");
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_btnIssueAllToAgency')]"))).Click();
            driver.SwitchTo().Alert().Accept();
            Thread.Sleep(2000);
            Assert.Contains("Agency", db.Execute("BOD2302_CRUD").Rows[0]["ResolveStatus_Display"].ToString());
            Thread.Sleep(5000);
        }
        [Fact]
        public void BOD2302__Admin_AddNew()
        { 
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Admin).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ds.ToUrl("~/Maintenance/BOD_2302_Admin.aspx");
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_AddNewRecordButton')]"))).Click();
            var na = new CSNaiveAutomator(context);
            na.ContainerSelector = "table[id$='_mainTable'] table tbody";
            na.Automate();
            var UniqueIdPoamId = Utils.ReverseRegex(this.config["UniqueIdPoamId"]);
            wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, 'UniqueIdPoamId')]"))).SendKeys(UniqueIdPoamId);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_PerformInsertButton')]"))).Click();
             
            Assert.Contains("Admin", db.Execute("BOD2302_CRUD").Rows[0]["ResolveStatus_Display"].ToString());
            db.Execute("DELETE FROM BOD2302 WHERE HostdeviceName='0'");
            Thread.Sleep(5000);
            driver.Quit();
        }

        [Fact]
        public void BOD2302__Agency_AddNew()
        { 
            var presentStatus = db.Execute("BOD2302_CRUD").Rows[0]["ResolveStatus_Display"].ToString();
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Agency).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            ds.ToTab("23-02");
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
  
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_AddNewRecordButton')]"))).Click();
            var UniqueIdPoamId = Utils.ReverseRegex(this.config["UniqueIdPoamId"]);
         
            var na = new CSNaiveAutomator(context);
            na.ContainerSelector = "table[id$='_mainTable'] table tbody";
            na.Automate();
            wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, 'UniqueIdPoamId')]"))).SendKeys(UniqueIdPoamId);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_PerformInsertButton')]"))).Click();

            Thread.Sleep(1000);
            Assert.True(true);
        }
        [Fact]
        public void BOD2302__Agency_Submit()
        {
            this.config = SettingsProvider.TestSettings["BOD2302"];
            driver = ds.CsConnect(UserContext.Agency).Driver;
            var context = new SessionContext(this._logger, driver)
            {
                Defaults = new DefaultInputProvider(driver).DefaultValues
            };
            ds.ToTab("23-02"); 
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2)); 
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'ctl04_EditButton')]"))).Click();
            var na = new CSNaiveAutomator(context);
            na.ContainerSelector = "table[id$='_mainTable'] table tbody";
            na.Automate();
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_UpdateButton')]"))).Click();

            ds.ToSection(1);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_btnSubmitForm')]"))).Click();
            Thread.Sleep(1000);
            driver.SwitchTo().Alert().Accept();
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_lb_Home')]"))).Click();
            Thread.Sleep(1000);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_btn_AgencySubmit')]"))).Click();

            Thread.Sleep(1000);
            Assert.Contains("Submitted", db.Execute("BOD2302_CRUD").Rows[0]["ResolveStatus_Display"].ToString());
             
        }

        [Fact]
        public void BOD2201KevCveUpload(){

            driver = ds.CsConnect(UserContext.Agency).Driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            ds.ToUrl("~/Maintenance/BOD_2201_Admin.aspx");
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
            Thread.Sleep(200);

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CVEimport')]"))).Click();
            Thread.Sleep(1000);

            driver.SwitchTo().Alert().Accept();
            Thread.Sleep(1000);
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");

            var ele = driver.FindElement(By.XPath("//div[@data-updated-count]")); 
            Assert.True(ele != null);

            ds.DisposeDriverService();

        }
         
        [Fact]
        public void BOD2201FormsSubmit()
        {
            this.config = SettingsProvider.TestSettings["BOD2201"];
            driver = ds.CsConnect(UserContext.Agency).Driver;

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            string response = "";
            string path = "";
            bool success = false;
            bool errorPresent = false;
            ds.ToUrl("~/Maintenance/BOD_2201_CDMimport.aspx");
            
            path = $"{ Utils.GetDownloadsPath() }\\BOD_2201_CDMimport_INVALID_AGENCY.csv";
            _logger.Information("/Maintenance/BOD_2201_CDMimport.aspx: {o}", new { Attempt = path });
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))).SendKeys(path);   
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]"))).Click();
            Thread.Sleep(2);

            response = "";
            var elements = driver.FindElements(By.XPath("//*[contains(@id, 'Error')]"));
            if (elements.Count > 0)
                response = elements[0].Text;
            _logger.Information("{o}", new { ErrorMessage = response });
            errorPresent = !string.IsNullOrWhiteSpace(response);
            if (!errorPresent)
                _logger.Warning("Expected Upload Error INVALID AGENCY: {o}", path );
            Assert.True(errorPresent);

            path = $"{Utils.GetDownloadsPath()}\\BOD_2201_CDMimport_VALID.csv";
            _logger.Information("/Maintenance/BOD_2201_CDMimport.aspx: {o}", new { Attempt = path }); 
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))).SendKeys(path);  
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]"))).Click();
            Thread.Sleep(5);

            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_lblUploadMessage')]")));
            if (ele != null) response = ele.Text;
            success = response.Contains("Success");
            if(!success)
                _logger.Warning("Upload Error: {o}", new { Attempt = path });
            Assert.True(success);
             
            _logger.Information("Submit BOD2201: {o}", new { Tab });
            ds.ToTab(Tab);
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"S1")); 

            ds.ToSection(1); // To Section 2
           
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CVERemediatedGrid_cb_NA')]/span")));
            var @class = ele.GetAttribute("class");
            if (!@class.Contains("checkbox-checked")) 
                ele.Click();
            Thread.Sleep(1);

            _logger.Information("BOD2201_2021_2_InValid: {o}", new { InvalidAttempt = config["BOD2201_2021_2_InValid"] });
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]"))); 
            ele.SendKeys(config["BOD2201_2021_2_InValid"]);   
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]")));
            ele.Click();
            Thread.Sleep(5);

            response = "";
            elements = driver.FindElements(By.XPath("//*[contains(@id, 'Error')]"));
            if (elements.Count > 0)
                response = elements[0].Text;

            _logger.Information("{o}", new { ErrorMessage = response });
            errorPresent = !string.IsNullOrWhiteSpace(response);
            if (!errorPresent)
                _logger.Warning("Expected Upload Error: {o}", config["BOD2201_2021_2_InValid"]);
            Assert.True(errorPresent);

            _logger.Information("BOD2201_2021_2_Valid: {o}", new { ValidAttempt = config["BOD2201_2021_2_Valid"] });
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_fileUpload')]")));
            ele.SendKeys(config["BOD2201_2021_2_Valid"]); 
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CBFileUploader_cmdUpload')]")));
            ele.Click();
            Thread.Sleep(5);

            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CVERemediatedGrid_lblUploadMessage')]")));
            response = ele.GetAttribute("innerText");
            _logger.Information("lblUploadMessage: {o}", new { Message = response });
            success = response.Contains("uploaded success"); 
            if (!success)
                _logger.Warning("Upload Error: {o}", config["BOD2201_2021_2_Valid"]);
            Assert.True(success);

            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_ctl05_EditButton')]"))).Click(); 
            
            string cnt = wait.Until(d => d.FindElement(By.XPath("//*[contains(@class, 'rgEditRow')]/td[contains(@class, 'ImportCountCol')]"))).GetAttribute("innerText");
            cnt = Regex.Replace(cnt, $@"[^\d]", "");
            if (!string.IsNullOrWhiteSpace(cnt))
            { 
                wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_ctl05_UnremediatedCount')]"))).Clear();
                wait.Until(d => d.FindElement(By.XPath("//textarea[contains(@id, '_ctl05_CveCountJustification')]"))).Clear(); 
                wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_ctl05_UnremediatedCount')]"))).SendKeys($"{cnt}");
            }
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_ctl05_UpdateButton')]"))).Click();
            Thread.Sleep(1);

            var validates = ds.FismaFormValidates();
            Assert.True(validates);

            if (validates) _logger.Information("FismaFormValidated {o}", new { Tab });
            //ds.DisposeDriverService(); 
        }
        #endregion
    }
}
