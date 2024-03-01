using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Arch.Unity.Jobs
{
    public unsafe struct NativeChunk : IDisposable
    {
        internal UnsafeHashMap<int, (IntPtr Ptr, int Length)> componentMap;
        int size;

        public int Size => size;
        public int ComponentCount => componentMap.Count;

        internal static NativeChunk Create(Chunk chunk, AllocatorManager.AllocatorHandle allocator, ref NativeList<GCHandle> gcHandles)
        {
            var nativeChunk = default(NativeChunk);
            nativeChunk.componentMap = new UnsafeHashMap<int, (IntPtr ptr, int length)>(chunk.Components.Length, allocator);
            nativeChunk.size = chunk.Size;

            foreach (var componentArray in chunk.Components)
            {
                var elementType = componentArray.GetType().GetElementType();
                if (!elementType.IsUnmanaged()) continue;

                var componentType = Core.Utils.Component.GetComponentType(elementType);
                var array = Unsafe.As<byte[]>(componentArray);
                var ptr = Unsafe.AsPointer(ref array[0]);
                gcHandles.Add(GCHandlePool.Create(componentArray));

                nativeChunk.componentMap.Add(componentType.Id, ((IntPtr)ptr, chunk.Size));
            }

            return nativeChunk;
        }

        public void Dispose()
        {
            componentMap.Dispose();
        }

        public unsafe NativeArray<T> GetNativeArray<T>(int componentId) where T : unmanaged
        {
            if (componentMap.TryGetValue(componentId, out var x))
            {
                var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)x.Ptr, x.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
                return array;
            }
            throw new ArgumentException($"Component ID:{componentId} not found.");
        }
    }
}