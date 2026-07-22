using OpenQA.Selenium;
using System;
using Xunit;
using System.Text.RegularExpressions;
using Serilog.Events; 
using CyberScope.Automator;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using CyberScope.Selenium;

namespace CyberScope.Tests.Selenium
{
    public class DataCall : BaseDataCallTest
    {
        #region CTOR  
        public DataCall(ITestOutputHelper output) : base(output)
        { 
        } 
        #endregion

        #region UNITTESTS   
        [Fact] 
        public void FormsValidate()
        { 
            base.ValidateFismaForm();
        }
        [Fact]
        public void FormsSubmit()
        {
            base.SubmitForms(); 
        }
        [Fact]
        public void FormsOpen()
        { 
            var driver = ds.CsConnect(UserContext.Agency).ToTab(config["Tab"]).Driver;
            ds.ToSection(qg => Regex.IsMatch(qg.SectionText, Sections, RegexOptions.IgnoreCase));
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5)); 
            var element = wait.Until(d => d.FindElement(By.XPath("//select[contains(@id, ddl_Sections)]")));
   
        } 

        #endregion
    }


}

