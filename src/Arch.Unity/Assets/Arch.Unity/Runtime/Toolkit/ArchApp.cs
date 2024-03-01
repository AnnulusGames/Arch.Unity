using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Arch.System;

namespace Arch.Unity.Toolkit
{
    public sealed partial class ArchApp : IDisposable
    {
        World world;
        Dictionary<ISystemRunner, Group<SystemState>> systemGroups;
        bool isRunning;

        internal object[] createInstanceParameterCache;

        public World World => world;
        public bool IsRunning => isRunning;

        ArchApp() { }
        public static ArchApp Create()
        {
            var world = World.Create();
            return Create(world);
        }

        public static ArchApp Create(World world)
        {
            var app = new ArchApp
            {
                world = world,
                systemGroups = new Dictionary<ISystemRunner, Group<SystemState>>(),
                createInstanceParameterCache = new object[] { world }
            };
            return app;
        }

        public void RegisterSystem(ISystem<SystemState> system)
        {
            RegisterSystem(system, SystemRunner.Default);
        }

        public void RegisterSystem(ISystem<SystemState> system, ISystemRunner runner)
        {
            if (!systemGroups.TryGetValue(runner, out var group))
            {
                group = new Group<SystemState>(runner.GetType().Name);
                systemGroups.Add(runner, group);
            }
            group.Add(system);

            if (IsRunning)
            {
                system.Initialize();
                runner.Register(system);
            }
        }

        public TSystem GetSystem<TSystem>() where TSystem : ISystem<SystemState>
        {
            return GetSystems<TSystem>().FirstOrDefault();
        }

        public IEnumerable<TSystem> GetSystems<TSystem>() where TSystem : ISystem<SystemState>
        {
            return systemGroups.SelectMany(x => x.Value.Find<TSystem>());
        }

        public IEnumerable<ISystem<SystemState>> GetAllSystems()
        {
            return systemGroups.SelectMany(x => x.Value.Find<ISystem<SystemState>>());
        }

        public ArchApp Run()
        {
            if (IsRunning) throw new InvalidOperationException("App is running.");

            isRunning = true;

            foreach (var kv in systemGroups)
            {
                kv.Value.Initialize();
                kv.Key.Register(kv.Value);
            }

            return this;
        }

        public void Stop()
        {
            if (!IsRunning) throw new InvalidOperationException("App is not running.");
            foreach (var kv in systemGroups)
            {
                kv.Key.Unregister(kv.Value);
            }
        }

        public void Dispose()
        {
            if (isRunning) Stop();
            world.Dispose();
        }
    }
}