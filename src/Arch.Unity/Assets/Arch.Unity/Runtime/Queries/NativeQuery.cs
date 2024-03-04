using System;
using Arch.Core;
using Arch.Core.Utils;
using Unity.Collections;
using UnityEngine;

namespace Arch.Unity.Queries
{
    public partial struct NativeQuery : IDisposable
    {
        private NativeArray<uint> _all;
        private NativeArray<uint> _any;
        private NativeArray<uint> _none;
        private NativeArray<uint> _exclusive;
        internal World World;

        public NativeQuery(
            Allocator allocator,
            NativeHashSet<ComponentType> all,
            NativeHashSet<ComponentType> any,
            NativeHashSet<ComponentType> none,
            NativeHashSet<ComponentType> exclusive,
            World world)
        {
            World = world;

            var allBitArray = new NativeBitArray(32, allocator);
            var anyBitArray = new NativeBitArray(32, allocator);
            var noneBitArray = new NativeBitArray(32, allocator);
            var exclusiveBitArray = new NativeBitArray(32, allocator);

            foreach (var componentType in all) allBitArray.Set(componentType.Id, true);
            foreach (var componentType in any) anyBitArray.Set(componentType.Id, true);
            foreach (var componentType in none) noneBitArray.Set(componentType.Id, true);
            foreach (var componentType in exclusive) exclusiveBitArray.Set(componentType.Id, true);

            _all = allBitArray.AsNativeArray<uint>();
            _any = anyBitArray.AsNativeArray<uint>();
            _none = noneBitArray.AsNativeArray<uint>();
            _exclusive = exclusiveBitArray.AsNativeArray<uint>();
        }

        internal NativeQuery And(NativeQuery other, Allocator allocator = Allocator.Temp)
        {
            var all = new NativeArray<uint>(Mathf.Max(_all.Length, other._all.Length), allocator);
            var any = new NativeArray<uint>(Mathf.Max(_any.Length, other._any.Length), allocator);
            var none = new NativeArray<uint>(Mathf.Max(_none.Length, other._none.Length), allocator);
            var exclusive = new NativeArray<uint>(Mathf.Max(_exclusive.Length, other._exclusive.Length), allocator);

            for (var i = 0; i < all.Length; i++) all[i] = _all[i] | other._all[i];
            for (var i = 0; i < any.Length; i++) any[i] = _any[i] | other._any[i];
            for (var i = 0; i < none.Length; i++) none[i] = _none[i] | other._none[i];
            for (var i = 0; i < exclusive.Length; i++) exclusive[i] = _exclusive[i] | other._exclusive[i];

            return new NativeQuery
            {
                _all = all,
                _any = any,
                _none = none,
                _exclusive = exclusive,
                World = World
            };
        }

        public NativeQueryArchetypeEnumerator GetEnumerator() => new(this, World.Archetypes.Span);

        internal bool Valid(in BitSet bitSet)
        {
            var bitSetSpan = bitSet.AsSpan();
            var hasExclusive = _exclusive.Length > 1 || _exclusive[0] != 0;
            var hasAll = _all.Length > 1 || _all[0] != 0;
            var hasAny = _any.Length > 1 || _any[0] != 0;
            var hasNone = _none.Length > 1 || _none[0] != 0;
            return hasExclusive
                ? BitsUtility.Exclusive(_exclusive.AsSpan(), bitSetSpan)
                : (!hasAll || BitsUtility.All(_all.AsSpan(), bitSetSpan))
                  && (!hasAny || BitsUtility.Any(_any.AsSpan(), bitSetSpan))
                  && (!hasNone || BitsUtility.None(_none.AsSpan(), bitSetSpan));
        }

        public void Dispose()
        {
            _all.Dispose();
            _any.Dispose();
            _none.Dispose();
            _exclusive.Dispose();
        }
    }
}