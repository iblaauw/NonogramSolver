using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Tests
{
    internal static class Utils
    {
        public static IEnumerable<TypeInfo> GetTypesWithAttribute<T>(Assembly assembly) where T : Attribute, new()
        {
            return GetTypesWithAttributeInternal(typeof(T), assembly);
        }

        public static IEnumerable<TypeInfo> GetTypesWithAttribute(Type attributeType, Assembly assembly)
        {
            if (attributeType == null)
                throw new ArgumentNullException();

            if (attributeType.IsAssignableFrom(typeof(Attribute)))
                throw new ArgumentException($"The given type {attributeType.FullName} must be an attribute");

            return GetTypesWithAttributeInternal(attributeType, assembly);
        }

        // TODO: this only looks at classes right now
        private static IEnumerable<TypeInfo> GetTypesWithAttributeInternal(Type attributeType, Assembly assembly)
        {
            // First check if this attribute is even valid to be on types at all. If it isn't, then we can shortcut this early
            var attributeUsage = attributeType.GetTypeInfo().GetCustomAttribute<AttributeUsageAttribute>(inherit: true);
            if (attributeUsage != null)
            {
                if (!attributeUsage.ValidOn.HasFlag(AttributeTargets.Class))
                {
                    return Enumerable.Empty<TypeInfo>();
                }
            }

            var searchableTypes = assembly.DefinedTypes.Where(t => t.IsClass);
            return searchableTypes.Where(t => t.IsDefined(attributeType));
        }
    }
}
