#if ARCH_UNITY_VCONTAINER_SUPPORT
using System;
using Arch.Core;
using Arch.Unity.Toolkit;
using VContainer;

namespace Arch.Unity
{
    internal sealed class ArchAppInstanceProvider : IInstanceProvider
    {
        readonly World world;
        readonly Action<ArchApp> initialization;
        ArchApp app;

        public ArchAppInstanceProvider(World world, Action<ArchApp> initialization = null)
        {
            this.world = world;
            this.initialization = initialization;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            if (app == null)
            {
                app = ArchApp.Create(world);
                initialization?.Invoke(app);
            }
            return app;
        }
    }
}
#endif