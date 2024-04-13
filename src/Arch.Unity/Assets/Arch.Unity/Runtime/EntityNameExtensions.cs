using Arch.Core;
using Unity.Collections;

namespace Arch.Unity
{
    public static class EntityNameExtensions
    {
        public static FixedString64Bytes GetName(this World world, Entity entity)
        {
            ref var name = ref world.Get<EntityName>(entity);
            return name.Value;
        }

        public static void SetName(this World world, Entity entity, FixedString64Bytes name)
        {
            world.Set<EntityName>(entity, new(name));
        }
    }
}