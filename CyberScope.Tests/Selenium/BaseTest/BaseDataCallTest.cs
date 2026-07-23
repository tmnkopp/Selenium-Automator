using Xunit;
using TinyCsvParser;
using CyberScope.Automator;
using Xunit;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using System;
using OpenQA.Selenium.Support.UI;

namespace CyberScope.Tests.Selenium
{
    public abstract class BaseDataCallTest : BaseTest
    {
        #region FIELDS  
        protected string Tab;
        protected string Sections;
        #endregion 

        #region CTOR
        public BaseDataCallTest(ITestOutputHelper output): base(output)
        { 
            this.Tab = config["Tab"];
            string sections = string.IsNullOrEmpty(config["Sections"]) ? ".*" : config["Sections"];
            this.Sections = sections;
        }
        #endregion 

        #region METHODS 
        protected void SubmitForms()
        {  
            string actualError = "";
            var driver = ds.CsConnect(UserContext.Agency).ToTab(Tab).Driver;
            _logger.Information("SubmitForms: {o}", new { Tab });
            ds.OnSectionComplete += (s, e) => {
                var elements = driver.FindElements(By.XPath("//*[contains(@id, 'Error')]")) ;
                if (elements.Count > 0)
                    actualError = elements[0].Text;
                bool submits = string.IsNullOrWhiteSpace(actualError);
                if (!submits)
                    _logger.Warning("Submit Error: {o}", new { Tab, e.Section.SectionText, actualError });
                else
                    _logger.Information("Submitted: {o}", new { Tab, e.Section.SectionText });
                Assert.True(submits);
            };
            ds.InitSections(qg => Regex.IsMatch(qg.SectionText, $"{Sections}", RegexOptions.IgnoreCase));
            if (SettingsProvider.appSettings["DisposeOnComplete"].ToLower() !="false" )
                ds.DisposeDriverService();
        }
        
        protected void ValidateFismaForm()
        { 
            _logger.Information("ValidateFismaForm: {o}", new { Tab });
            var driver = ds.CsConnect(UserContext.Agency).ToTab(Tab).Driver;

            ds.ToSection(-1);
            var elements = new WebDriverWait(ds.Driver, TimeSpan.FromSeconds(5))
            .Until(dvr => dvr.FindElements(By.CssSelector("#ctl00_ContentPlaceHolder1_lblSuccessInfo")));
            Assert.True(elements.Count > 0, $"FISMA Form Validation Failed for Tab: {Tab}"); 
            ds.DisposeDriverService(); 
        }
 
        #endregion
         
    }
}
