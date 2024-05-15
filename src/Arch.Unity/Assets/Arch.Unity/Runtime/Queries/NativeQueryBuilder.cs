using System;
using Arch.Core;
using Arch.Core.Utils;
using Unity.Collections;

namespace Arch.Unity.Queries
{
    public partial struct NativeQueryBuilder : IDisposable
    {
        private readonly World _world;
        private NativeHashSet<ComponentType> _all;
        private NativeHashSet<ComponentType> _any;
        private NativeHashSet<ComponentType> _none;
        private NativeHashSet<ComponentType> _exclusive;

        public NativeQueryBuilder(World world, Allocator allocator)
        {
            _world = world;
            _all = new NativeHashSet<ComponentType>(1, allocator);
            _any = new NativeHashSet<ComponentType>(1, allocator);
            _none = new NativeHashSet<ComponentType>(1, allocator);
            _exclusive = new NativeHashSet<ComponentType>(1, allocator);
        }

        public NativeQuery Build(Allocator allocator = Allocator.Temp)
        {
            return new NativeQuery(allocator, _all, _any, _none, _exclusive, _world);
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