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
            if (!_isUnmanagedCache.TryGetValue(type, out bool result))
            {
                if (!type.IsValueType)
                {
                    result = false;
                }
                else if (type.IsPrimitive || type.IsPointer || type.IsEnum)
                {
                    result = true;
                }
                else
                {
                    result = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .All(f => IsUnmanaged(f.FieldType));
                }

                _isUnmanagedCache[type] = result;
            }

            return result;
        }
    }
}