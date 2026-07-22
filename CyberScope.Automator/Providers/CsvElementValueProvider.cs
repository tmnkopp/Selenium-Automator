using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;

namespace CyberScope.Automator.Providers
{
    public class CsvElementValueProvider : ElementValueProvider
    {
        public override void Populate(SessionContext sessionContext)
        {
            // base.Populate(ds);
            string _fileName = Utils.GetDataPath() + "ElementPath.csv";
            var csvParser = new CsvParser<ElementValueMap>(new CsvParserOptions(true, ','), new ElementValueMapper());
            var result = csvParser.ReadFromFile(_fileName, Encoding.ASCII).ToList();
            foreach (var item in result)
            {
                var row = item.Result.GetAsRow;
                var elm = sessionContext.Driver.FindElement(By.XPath((string)row[0]));
                if (!string.IsNullOrEmpty(elm.Text))
                    SetMetric((string)row[1], elm.Text);
            }
            var ans = this._answers; 
        }
    }
}
