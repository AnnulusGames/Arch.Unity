using System.Collections.Generic;
using Unity.Collections;

namespace Arch.Unity
{
    internal static class AllocatorPool
    {
        static readonly Stack<AllocatorHelper<RewindableAllocator>> pool = new();

        public static AllocatorHelper<RewindableAllocator> Rent()
        {
            if (!pool.TryPop(out var allocatorHelper))
            {
                allocatorHelper = new AllocatorHelper<RewindableAllocator>(Allocator.TempJob);
                allocatorHelper.Allocator.Initialize(128 * 1024, true);
            }
            return allocatorHelper;
        }

        public static void FreeAndReturn(AllocatorHelper<RewindableAllocator> allocatorHelper)
        {
            allocatorHelper.Allocator.Rewind();
            pool.Push(allocatorHelper);
        }
    }
}