using OpenQA.Selenium;
using System; 
using Xunit;
using System.Text.RegularExpressions; 
using Serilog;
using Serilog.Events;
using Xunit;
using CyberScope.Automator;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.PeerToPeer;

namespace CyberScope.Tests.Selenium
{ 
    public class User : BaseTest
    {
        #region FIELDS  
        Dictionary<string,string> config;
        #endregion

        #region CTOR 
        public User(ITestOutputHelper output): base(output)
        {
            config = SettingsProvider.TestSettings["UserMgmt"];
        }
        #endregion

        #region UNITTESTS   
        [Fact]
        public void AgencyConnects()
        {
            var driver = ds.CsConnect(UserContext.Agency).Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2)); 
            Assert.True(driver.Title.Contains("CyberScope"));  
            ds.DisposeDriverService();
        }
        [Fact]
        public void AdminConnects()
        {
            var driver = ds.CsConnect(UserContext.Admin).Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
             
            ds.DisposeDriverService();
           
        }
        [Fact]
        public void SSOConnects()
        {
            var driver = ds.CsConnect(UserContext.Admin).Driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ds.ToUrl("~/Emulate.aspx");
            IWebElement ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'hlRedirectToScheduler')]")));
            ele.Click(); 
        }
        [Fact]
        public void USER_ACCESS_UPDATES()
        {
            var driver = ds.CsConnect(UserContext.Admin).Driver;
            IWebElement ele;
            IReadOnlyCollection<IWebElement> eles;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
             
            ds.ToUrl("~/UserAccessNew/SelectUser.aspx");
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_WebTextEdit1')]")));
            ele.SendKeys(config["User"]);
            wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, 'ctl00_ContentPlaceHolder1_btn_Run')]"))).Click();
            wait.Until(d => d.FindElement(By.XPath("//a[contains(@id, '_ctl04_link_UserID')]"))).Click();
            driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);
   
            wait.Until(d => d.FindElement(By.XPath("//span[contains(text(),'Access Codes')]"))).Click();
            wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_btnClearAll')]"))).Click();
            driver.SwitchTo().Alert().Accept();
            wait.Until(d => d.FindElement(By.XPath("//span[contains(text(),'Agency Access')]/../input"))).Click();
            wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_lb_Save')]"))).Click();
            ele = wait.Until(d => d.FindElement(By.XPath("//span[contains(@id, '_lblCertStatusDateValue')]"))); 
            Assert.NotNull(ele);
            wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, '_CancelButton')]"))).Click();
            driver.SwitchTo().Window(driver.WindowHandles[0]);
            wait.Until(d => d.FindElement(By.XPath("//a[contains(text(),'LOG OFF')]"))).Click();
            ds.DisposeDriverService();
        }
        [Fact] 
        public void USER_AGENCY_UPDATES()
        { 
            var driver = ds.CsConnect(UserContext.Admin).Driver; 
            IWebElement ele;
            IReadOnlyCollection<IWebElement> eles;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            
            ds.ToUrl("~/UserAccessNew/SelectUser.aspx"); 
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_WebTextEdit1')]")));
            ele.SendKeys(config["User"]); 
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, 'ctl00_ContentPlaceHolder1_btn_Run')]"))); 
            ele.Click();      
            ele = wait.Until(d => d.FindElement(By.XPath("//a[contains(@id, '_ctl04_link_UserID')]"))); 
            ele.Click(); 
            driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count-1]); 
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_Profile_btn_Edit')]"))); 
            ele.Click();            
            ele = wait.Until(d => d.FindElement(By.XPath("//div[contains(@id, '_ddl_Component')]"))); 
            ele.Click();
            Thread.Sleep(1);
            ((IJavaScriptExecutor)driver).ExecuteScript("scroll(0, 250)"); 
            Thread.Sleep(1);
            eles = wait.Until(d => d.FindElements(By.XPath("//ul[contains(@class, 'rddlList')]/li"))); 
            ele = (from e in eles where Regex.IsMatch(e.Text.Trim(), config["Agency"].Trim(), RegexOptions.IgnoreCase) select e).FirstOrDefault();
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", ele); 
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_Profile_btn_Save')]")));
            ele.Click(); 
            ele = wait.Until(d => d.FindElement(By.XPath("//span[contains(@id, '_Profile_OrgSubOrg_lbl_Agency')]")));
            var UpdatedAgency = ele.Text.Trim().ToLower();
            var pass = UpdatedAgency.Contains(config["Agency"].Trim().ToLower());
            if (pass) _logger.Information("USER_AGENCY_UPDATED: {o}", new { UpdatedAgency }) ;
            Assert.True(pass);
  
            ds.ToUrl("~/ReporterHome.aspx"); 
             
            ds.DisposeDriverService(); 
        }
        [Fact]
        public void PASSWORD_RESETS()
        {

            updatePassword(SettingsProvider.appSettings["AdminUser"], SettingsProvider.appSettings["AdminPass"], UserContext.Admin);
            Assert.True(ds.Driver.Url.ToLower().Contains("home.aspx"));

            updatePassword(SettingsProvider.appSettings["AgencyUser"], SettingsProvider.appSettings["AgencyPass"], UserContext.Agency); 
            Assert.True(ds.Driver.Url.ToLower().Contains("home.aspx")); 
        
            ds.DisposeDriverService();

        }

        private void updatePassword(string username, string password, UserContext userContext)
        {
            IWebElement ele;
            var driver = ds.Driver;
            ds.ToUrl(SettingsProvider.appSettings[$"CSTargerUrl"]);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
            ds.ToUrl("~/Login.aspx");
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'LinkButton2')]")));
            ele.Click();
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_UserName')]")));
            ele.Clear();
            ele.SendKeys(username);
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_CurrentPassword')]")));
            ele.SendKeys(password);
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_NewPassword')]")));
            ele.SendKeys(password + "1");
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_ConfirmNewPassword')]")));
            ele.SendKeys(password + "1");
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_ChangePasswordPushButton')]")));
            ele.Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until((x) =>
            {
                return ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
            });

            ds.ToUrl("~/Login.aspx");
            ele = wait.Until(d => d.FindElement(By.XPath("//*[contains(@id, 'LinkButton2')]")));
            ele.Click();

            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_UserName')]")));
            ele.Clear();
            ele.SendKeys(username);
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_CurrentPassword')]")));
            ele.SendKeys(password + "1");
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_NewPassword')]")));
            ele.SendKeys(password);
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_ConfirmNewPassword')]")));
            ele.SendKeys(password);
            ele = wait.Until(d => d.FindElement(By.XPath("//input[contains(@id, '_ChangePasswordPushButton')]")));
            ele.Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until((x) =>
            {
                return ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
            });

            ds.ToUrl("~/Login.aspx");
            ds.CsConnect(userContext); 
        }
        #endregion
    } 
} 