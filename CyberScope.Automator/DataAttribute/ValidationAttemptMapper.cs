using CyberScope.Automator;
using System.Collections.Generic;
using TinyCsvParser.Mapping;

namespace CyberScope.Automator
{
    public class ValidationAttemptMapper : CsvMapping<ValidationAttempt>
    {
        public ValidationAttemptMapper() : base()
        {
            MapProperty(0, x => x.Section);
            MapProperty(1, x => x.MetricXpath);
            MapProperty(2, x => x.ErrorAttemptExpression);
            MapProperty(3, x => x.ExpectedError); 
        }
    }
}
