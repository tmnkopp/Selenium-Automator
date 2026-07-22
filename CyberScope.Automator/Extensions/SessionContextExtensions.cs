using CyberScope.Automator.Providers;
using System.Linq;

namespace CyberScope.Automator
{
    public static class SessionContextExtensions
    {
        public static void RefreshDefaults(this SessionContext context)
        {
            context.Defaults = new DefaultInputProvider(context.Driver).DefaultValues;
        }
        
        public static void RefreshDefaults(this SessionContext context, IElementValueProvider elementValueProvider)
        {
            var provider = new DefaultInputProvider(context.Driver);
            context.Defaults = provider.DefaultValues;
            elementValueProvider?.Populate(context);
            
            // Apply metrics to defaults
            foreach (var key in context.Defaults.Keys.ToArray())
            {
                if (context.Defaults[key].Contains("{"))
                    context.Defaults[key] = elementValueProvider.Eval<string>(context.Defaults[key]);
            }
        }
    }
}