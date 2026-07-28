using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.System;
using UnityEngine;

namespace Arch.Unity.Toolkit
{

    /// <summary>
    /// Implemented by systems whose runtime type name is ambiguous in the Profiler
    /// (e.g. many instances of the same generic type). The aggregator uses
    /// <see cref="ProfilerName"/> for the per-system ProfilerMarker instead of the type name.
    /// </summary>
    public interface IProfilerNamed
    {
        string ProfilerName { get; }
    }

    public sealed class ProfiledSystemRunner : ISystemRunner
    {
        public ProfiledSystemRunner(PlayerLoopTiming timing)
        {
            this.timing = timing;
        }

        readonly PlayerLoopTiming timing;
        readonly List<UnitySystemBase> systems = new();
        readonly Dictionary<UnitySystemBase, string> markerNames = new();

        public PlayerLoopTiming Timing => timing;

        public void Add(UnitySystemBase system)
        {
            systems.Add(system);
            if (!markerNames.ContainsKey(system))
            {

                string name = system is IProfilerNamed named
                    ? named.ProfilerName
                    : system.GetType().Name;
                markerNames[system] = name;
            }
        }

        public void Remove(UnitySystemBase system)
        {
            systems.Remove(system);
            markerNames.Remove(system);
        }

        public void Run()
        {
            var state = new SystemState()
            {
                Time = GetElaspedTime(timing),
                DeltaTime = GetDeltaTime(timing),
            };

            foreach (var system in systems)
            {
                UnityEngine.Profiling.Profiler.BeginSample(markerNames[system]);
                try
                {
                    try { system.BeforeUpdate(state); }
                    catch (Exception ex) { Debug.LogException(ex); }
                    try { system.Update(state); }
                    catch (Exception ex) { Debug.LogException(ex); }
                    try { system.AfterUpdate(state); }
                    catch (Exception ex) { Debug.LogException(ex); }
                }
                finally
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                }
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
