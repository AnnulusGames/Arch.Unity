using System;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using PlayerLoopType = UnityEngine.PlayerLoop;
using Arch.Unity.Jobs;
using Arch.Unity.Toolkit;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arch.Unity
{
    public static class ArchLoopRunners
    {
        public struct ArchInitialization { };
        public struct ArchEarlyUpdate { };
        public struct ArchFixedUpdate { };
        public struct ArchPreUpdate { };
        public struct ArchUpdate { };
        public struct ArchPreLateUpdate { };
        public struct ArchPostLateUpdate { };
        public struct ArchTimeUpdate { };
    }

    public enum PlayerLoopTiming
    {
        Initialization = 0,
        EarlyUpdate = 1,
        FixedUpdate = 2,
        PreUpdate = 3,
        Update = 4,
        PreLateUpdate = 5,
        PostLateUpdate = 6,
        TimeUpdate = 7,
    }

    internal static class PlayerLoopHelper
    {
        public static event Action OnInitialization;
        public static event Action OnEarlyUpdate;
        public static event Action OnFixedUpdate;
        public static event Action OnPreUpdate;
        public static event Action OnUpdate;
        public static event Action OnPreLateUpdate;
        public static event Action OnPostLateUpdate;
        public static event Action OnTimeUpdate;

        static bool initialized;
        static bool eventsInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            if (!eventsInitialized)
            {
                // Initialize JobArchChunkHandle
                JobArchChunkHandle.Initialize();
                OnUpdate += JobArchChunkHandle.CheckHandles;

                // Initialize Apps
                OnInitialization += SystemRunner.Initialization.Run;
                OnEarlyUpdate += SystemRunner.EarlyUpdate.Run;
                OnFixedUpdate += SystemRunner.FixedUpdate.Run;
                OnPreUpdate += SystemRunner.PreUpdate.Run;
                OnUpdate += SystemRunner.Update.Run;
                OnPreLateUpdate += SystemRunner.PreLateUpdate.Run;
                OnPostLateUpdate += SystemRunner.PostLateUpdate.Run;
                OnTimeUpdate += SystemRunner.TimeUpdate.Run;

                eventsInitialized = true;
            }
            
#if UNITY_EDITOR
            var domainReloadDisabled = EditorSettings.enterPlayModeOptionsEnabled && EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload);
            if (!domainReloadDisabled && initialized) return;
#else
            if (initialized) return;
#endif

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            Initialize(ref playerLoop);
        }

        public static void Initialize(ref PlayerLoopSystem playerLoop)
        {
            initialized = true;
            var newLoop = playerLoop.subSystemList.ToArray();

            InsertLoop(newLoop, typeof(PlayerLoopType.Initialization), typeof(ArchLoopRunners.ArchInitialization), static () => OnInitialization?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.EarlyUpdate), typeof(ArchLoopRunners.ArchEarlyUpdate), static () => OnEarlyUpdate?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.FixedUpdate), typeof(ArchLoopRunners.ArchFixedUpdate), static () => OnFixedUpdate?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.PreUpdate), typeof(ArchLoopRunners.ArchPreUpdate), static () => OnPreUpdate?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.Update), typeof(ArchLoopRunners.ArchUpdate), static () => OnUpdate?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.PreLateUpdate), typeof(ArchLoopRunners.ArchPreLateUpdate), static () => OnPreLateUpdate?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.PostLateUpdate), typeof(ArchLoopRunners.ArchPostLateUpdate), static () => OnPostLateUpdate?.Invoke());
            InsertLoop(newLoop, typeof(PlayerLoopType.TimeUpdate), typeof(ArchLoopRunners.ArchTimeUpdate), static () => OnTimeUpdate?.Invoke());

            playerLoop.subSystemList = newLoop;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void InsertLoop(PlayerLoopSystem[] loopSystems, Type loopType, Type loopRunnerType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            var i = FindLoopSystemIndex(loopSystems, loopType);
            ref var loop = ref loopSystems[i];
            loop.subSystemList = InsertRunner(loop.subSystemList, loopRunnerType, updateDelegate);
        }

        static int FindLoopSystemIndex(PlayerLoopSystem[] playerLoopList, Type systemType)
        {
            for (int i = 0; i < playerLoopList.Length; i++)
            {
                if (playerLoopList[i].type == systemType)
                {
                    return i;
                }
            }

            throw new Exception("Target PlayerLoopSystem does not found. Type:" + systemType.FullName);
        }

        static PlayerLoopSystem[] InsertRunner(PlayerLoopSystem[] subSystemList, Type loopRunnerType, PlayerLoopSystem.UpdateFunction updateDelegate)
        {
            var source = subSystemList.Where(x => x.type != loopRunnerType).ToArray();
            var dest = new PlayerLoopSystem[source.Length + 1];

            Array.Copy(source, 0, dest, 1, source.Length);

            dest[0] = new PlayerLoopSystem
            {
                type = loopRunnerType,
                updateDelegate = updateDelegate
            };

            return dest;
        }
    }
}