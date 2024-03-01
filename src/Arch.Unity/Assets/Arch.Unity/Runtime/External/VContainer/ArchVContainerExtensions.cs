#if ARCH_UNITY_VCONTAINER_SUPPORT
using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using Arch.Unity.Toolkit;
using VContainer;

namespace Arch.Unity
{
    public static class ArchVContainerExtensions
    {
        public readonly struct NewArchAppBuilder
        {
            readonly IContainerBuilder containerBuilder;

            public NewArchAppBuilder(IContainerBuilder containerBuilder)
            {
                this.containerBuilder = containerBuilder;
            }

            public void Add<T>() where T : ISystem<SystemState>
            {
                containerBuilder.RegisterSystemIntoArchApp<T>();
            }

            public void Add<T>(ISystemRunner systemRunner) where T : ISystem<SystemState>
            {
                containerBuilder.RegisterSystemIntoArchApp<T>(systemRunner);
            }
        }

        public static RegistrationBuilder RegisterArchApp(this IContainerBuilder builder, ArchApp app)
        {
            return builder.RegisterInstance(app);
        }

        public static RegistrationBuilder RegisterSystemFromArchApp<T>(this IContainerBuilder builder, ArchApp app)
            where T : ISystem<SystemState>
        {
            return builder.RegisterInstance(app.GetSystem<T>());
        }

        public static RegistrationBuilder RegisterNewArchApp(this IContainerBuilder builder, Lifetime lifetime, Action<ArchApp> configuration = null)
        {
            return RegisterNewArchApp(builder, lifetime, null, configuration);
        }

        public static RegistrationBuilder RegisterNewArchApp(this IContainerBuilder builder, Lifetime lifetime, World world, Action<ArchApp> configuration = null)
        {
            builder.RegisterBuildCallback(resolver =>
            {
                var app = resolver.Resolve<ArchApp>();
                var systems = resolver.Resolve<IEnumerable<ISystem<SystemState>>>();
                foreach (var system in systems)
                {
                    app.RegisterSystem(system);
                }
                app.Run();
            });

            builder.Register(resolver =>
            {
                var app = resolver.Resolve<ArchApp>();
                return app.World;
            }, lifetime);
            
            var registerationBuilder = new ArchAppRegistrationBuilder(lifetime, world, configuration);
            return builder.Register(registerationBuilder);
        }

        public static RegistrationBuilder RegisterSystemIntoArchApp<T>(this IContainerBuilder builder)
            where T : ISystem<SystemState>
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), SystemRunner.Default);
            builder.Register(registrationBuilder).As<ISystem<SystemState>>();
            return builder.Register(registrationBuilder);
        }

        public static RegistrationBuilder RegisterSystemIntoArchApp<T>(this IContainerBuilder builder, ISystemRunner systemRunner)
            where T : ISystem<SystemState>
        {
            var registrationBuilder = new SystemRegistrationBuilder(typeof(T), systemRunner);
            builder.Register(registrationBuilder).As<ISystem<SystemState>>();
            return builder.Register(registrationBuilder);
        }

        public static void UseNewArchApp(this IContainerBuilder builder, Lifetime lifetime, World world, Action<NewArchAppBuilder> configuration)
        {
            builder.RegisterNewArchApp(lifetime, world);
            configuration(new NewArchAppBuilder(builder));
        }

        public static void UseNewArchApp(this IContainerBuilder builder, Lifetime lifetime, Action<NewArchAppBuilder> configuration)
        {
            builder.RegisterNewArchApp(lifetime);
            configuration(new NewArchAppBuilder(builder));
        }
    }
}
#endif