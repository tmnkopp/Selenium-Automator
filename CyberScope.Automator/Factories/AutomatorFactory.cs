using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberScope.Automator
{
    public class AutomatorFactory
    {
        private readonly SessionContext _sessionContext;
        private static readonly Dictionary<string, Type> CachedAutomatorTypes;
        private static readonly Dictionary<string, Type> CachedSetterTypes;

        static AutomatorFactory()
        {
            var targetAssembly = typeof(AutomatorFactory).Assembly;

            CachedAutomatorTypes = targetAssembly.GetExportedTypes()
                .Where(t => typeof(IAutomator).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);

            CachedSetterTypes = targetAssembly.GetExportedTypes()
                .Where(t => typeof(IValueSetter).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
        }

        public AutomatorFactory(SessionContext sessionContext)
        {
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
        }

        public IAutomator Create(string automatorTypeName)
        {
            if (!CachedAutomatorTypes.TryGetValue(automatorTypeName, out Type automatorType))
            {
                throw new TypeLoadException($"Framework Error: Automator class '{automatorTypeName}' was not found in cache.");
            }

            if (typeof(ControlAutomator).IsAssignableFrom(automatorType))
            {
                var resolvedSetters = CachedSetterTypes.Values
                    .Select(t => (IValueSetter)Activator.CreateInstance(t))
                    .ToList();

                return (IAutomator)Activator.CreateInstance(automatorType, _sessionContext, resolvedSetters);
            }

            return (IAutomator)Activator.CreateInstance(automatorType, _sessionContext);
        }
    }
}
