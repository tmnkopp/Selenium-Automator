using CyberScope.Automator;
using TinyCsvParser.Mapping;

namespace CyberScope.Automator.Providers
{
    public class ElementValueMapper : CsvMapping<ElementValueMap>
    {
        public ElementValueMapper() : base()
        {
            MapProperty(0, x => x.SRCXPATH); 
            MapProperty(1, x => x.DESTXPATH);
        }
    }
}
