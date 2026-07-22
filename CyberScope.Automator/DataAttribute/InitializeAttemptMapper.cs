using CyberScope.Automator;
using TinyCsvParser.Mapping;

namespace CyberScope.Automator
{
    public class InitializeAttemptMapper : CsvMapping<InitializeAttempt>
    {
        public InitializeAttemptMapper() : base()
        {
            MapProperty(0, x => x.Tab); 
            MapProperty(1, x => x.Section); 
        }
    }
}
