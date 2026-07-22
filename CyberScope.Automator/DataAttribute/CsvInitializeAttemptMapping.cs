using CyberScope.Automator;
using TinyCsvParser.Mapping;

namespace CyberScope.Automator
{
    public class CsvInitializeAttemptMapping : CsvMapping<InitializeAttempt>
    {
        public CsvInitializeAttemptMapping() : base()
        {
            MapProperty(0, x => x.Section); 
        }
    }
}
