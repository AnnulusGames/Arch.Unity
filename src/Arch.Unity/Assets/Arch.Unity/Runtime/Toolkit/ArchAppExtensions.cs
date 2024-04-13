using System;
using Arch.Core;
using Arch.System;

namespace Arch.Unity.Toolkit
{
    public static class ArchAppExtensions
    {
        public readonly struct ArchAppSystemBuilder
        {
            public ArchAppSystemBuilder(ArchApp app, ISystemRunner systemRunner)
            {
                this.app = app;
                this.systemRunner = systemRunner;
            }

            readonly ArchApp app;
            readonly ISystemRunner systemRunner;

            public void Add(UnitySystemBase system)
            {
                app.RegisterSystem(system, systemRunner);
            }

            public void Add<T>() where T : UnitySystemBase
            {
                var parameters = app.createInstanceParameterCache;
                var system = (T)Activator.CreateInstance(typeof(T), parameters);
                app.RegisterSystem(system, systemRunner);
            }
        }

        public static ArchApp AddSystems(this ArchApp app, Action<ArchAppSystemBuilder> configuration)
        {
            configuration(new ArchAppSystemBuilder(app, SystemRunner.Default));
            return app;
        }

        public static ArchApp AddSystems(this ArchApp app, ISystemRunner systemRunner, Action<ArchAppSystemBuilder> configuration)
        {
            configuration(new ArchAppSystemBuilder(app, systemRunner));
            return app;
        }
    }
}