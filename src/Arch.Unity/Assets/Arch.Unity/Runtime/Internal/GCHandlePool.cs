using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Arch.Unity
{
    internal static class GCHandlePool
    {
        static readonly Stack<GCHandle> stack = new();

        public static GCHandle Create<T>(T value)
        {
            if (stack.Count == 0)
            {
                return GCHandle.Alloc(value, GCHandleType.Pinned);
            }
            else
            {
                var ret = stack.Pop();
                ret.Target = value;
                return ret;
            }
        }

        public static void Release(GCHandle value)
        {
            if (value.IsAllocated)
            {
                value.Target = null;
                stack.Push(value);
            }
        }

        public static void Free()
        {
            while (!stack.TryPop(out var handle)) handle.Free();
        }
    }
}