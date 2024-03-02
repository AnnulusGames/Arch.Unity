using System;
using System.Collections.Generic;
using Arch.System;
using UnityEngine;

namespace Arch.Unity.Toolkit
{
    // TODO: optimize register/unregister

    public sealed class FakeSystemRunner : ISystemRunner
    {
        readonly List<ISystem<SystemState>> systems = new();

        public double Time { get; set; } = 0.0;
        public float DeltaTime { get; set; } = 0.1f;

        public void Add(ISystem<SystemState> system)
        {
            systems.Add(system);
        }

        public void Remove(ISystem<SystemState> system)
        {
            systems.Remove(system);
        }

        public void Run()
        {
            Time += DeltaTime;

            var state = new SystemState()
            {
                Time = Time,
                DeltaTime = DeltaTime,
            };

            foreach (var system in systems)
            {
                try { system.BeforeUpdate(state); }
                catch (Exception ex) { Debug.LogException(ex); }
                try { system.Update(state); }
                catch (Exception ex) { Debug.LogException(ex); }
                try { system.AfterUpdate(state); }
                catch (Exception ex) { Debug.LogException(ex); }
            }
        }
    }
}