using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Arch.Unity
{
    internal static class TypeExtensions
    {
        static readonly ConcurrentDictionary<Type, bool> _isUnmanagedCache = new();

        public static bool IsUnmanaged(this Type type)
        {
            if (_isUnmanagedCache.TryGetValue(type, out bool result))
            {
                return result;
            }
            
			if (type.IsPrimitive || type.IsEnum || type.IsPointer)
			{
				result = true;
			}
			else if (type.IsValueType)
			{
				result = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
							 .All(field => field.FieldType.IsUnmanaged());
			}
			else
			{
				result = false;
			}

			_isUnmanagedCache[type] = result;
			return result;
        }
    }
}
