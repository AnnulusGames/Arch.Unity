using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Arch.System;
using System;

namespace Arch.Unity.Toolkit
{
    // TODO: optimize register/unregister

    public sealed class SystemRunner : ISystemRunner
    {
        static SystemRunner()
        {
            Initialization = new SystemRunner(PlayerLoopTiming.Initialization);
            EarlyUpdate = new SystemRunner(PlayerLoopTiming.EarlyUpdate);
            FixedUpdate = new SystemRunner(PlayerLoopTiming.FixedUpdate);
            PreUpdate = new SystemRunner(PlayerLoopTiming.PreUpdate);
            Update = new SystemRunner(PlayerLoopTiming.Update);
            PreLateUpdate = new SystemRunner(PlayerLoopTiming.PreLateUpdate);
            PostLateUpdate = new SystemRunner(PlayerLoopTiming.PostLateUpdate);
            TimeUpdate = new SystemRunner(PlayerLoopTiming.TimeUpdate);

            Default = Update;
        }

        SystemRunner(PlayerLoopTiming playerLoopTiming)
        {
            this.playerLoopTiming = playerLoopTiming;
        }

        readonly PlayerLoopTiming playerLoopTiming;
        readonly List<ISystem<SystemState>> systems = new();

        public static ISystemRunner Default { get; set; }

        public static readonly ISystemRunner Initialization;
        public static readonly ISystemRunner EarlyUpdate;
        public static readonly ISystemRunner FixedUpdate;
        public static readonly ISystemRunner PreUpdate;
        public static readonly ISystemRunner Update;
        public static readonly ISystemRunner PreLateUpdate;
        public static readonly ISystemRunner PostLateUpdate;
        public static readonly ISystemRunner TimeUpdate;

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
            var state = new SystemState()
            {
                Time = GetElaspedTime(playerLoopTiming),
                DeltaTime = GetDeltaTime(playerLoopTiming),
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float GetDeltaTime(PlayerLoopTiming playerLoopTiming)
        {
            return playerLoopTiming == PlayerLoopTiming.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double GetElaspedTime(PlayerLoopTiming playerLoopTiming)
        {
            return playerLoopTiming == PlayerLoopTiming.FixedUpdate ? Time.fixedTimeAsDouble : Time.timeAsDouble;
        }
    }
}