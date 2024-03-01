#if ARCH_UNITY_VCONTAINER_SUPPORT
using System;
using Arch.Core;
using Arch.Unity.Toolkit;
using VContainer;

namespace Arch.Unity
{
    internal sealed class ArchAppRegistrationBuilder : RegistrationBuilder
    {
        readonly World world;
        readonly Action<ArchApp> initialization;

        public ArchAppRegistrationBuilder(Lifetime lifetime, World world, Action<ArchApp> initialization)
            : base(typeof(ArchApp), lifetime)
        {
            this.world = world;
            this.initialization = initialization;
        }

        public override Registration Build()
        {
            var provider = new ArchAppInstanceProvider(world, initialization);
            return new Registration(typeof(ArchApp), Lifetime, null, provider);
        }
    }
}
#endif