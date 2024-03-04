using System;
using Arch.Core;
using Unity.Collections;

namespace Arch.Unity.Queries
{
    public static partial class WorldExtensions
    {
        public static NativeQueryBuilder CreateNativeQueryBuilder(this World world,
            Allocator allocator = Allocator.Temp) =>
            new(world, allocator);

        public static ref T GetSingleton<T>(this World world) =>
            ref world.CreateNativeQueryBuilder().Build().GetSingleton<T>();

        public static ref T GetSingleton<T>(this NativeQuery query)
        {
            var secondQuery = query.World.CreateNativeQueryBuilder().WithAll<T>().Build();
            var enumerator = query.And(secondQuery).GetEnumerator();
            if (!enumerator.MoveNext()) throw new InvalidOperationException("No singleton found");
            var chunks = enumerator.Current.Chunks;
            if (enumerator.Current.Entities != 1) throw new InvalidOperationException("More than one singleton found");
            return ref chunks[0].Get<T>(0);
        }
    }
}