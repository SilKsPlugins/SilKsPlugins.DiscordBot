using Autofac.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilKsPlugins.DiscordBot.Helpers
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> FindAllTypes(this Assembly assembly, bool includeAbstractAndInterfaces = false)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.GetLoadableTypes()
                    .Where(c => includeAbstractAndInterfaces || !c.IsAbstract && !c.IsInterface);
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).Select(x => x!);
            }
        }

        public static IEnumerable<Type> FindTypes<T>(this Assembly assembly, bool includeAbstractAndInterfaces = false)
        {
            return assembly.FindAllTypes(includeAbstractAndInterfaces).Where(c => typeof(T).IsAssignableFrom(c));
        }
    }
}
