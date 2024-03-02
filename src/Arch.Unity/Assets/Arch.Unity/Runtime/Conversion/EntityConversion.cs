using UnityEngine;
using Arch.Core;

namespace Arch.Unity.Conversion
{
    public static class EntityConversion
    {
#if !ARCH_UNITY_DISABLE_AUTOMATIC_BOOTSTRAP
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            var world = DefaultWorld = World.Create();

            Application.quitting += () =>
            {
                if (World.Worlds[world.Id] != null) world.Dispose();
            };
        }
#endif

        public static World DefaultWorld { get; set; }
    }
}