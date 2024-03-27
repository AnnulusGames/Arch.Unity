using System.Runtime.CompilerServices;
using UnityEngine;
using Arch.Core;

namespace Arch.Unity.Conversion
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class SyncWithEntity : MonoBehaviour
    {
        internal World World { get; set; }
        internal EntityReference EntityReference { get; set; }
        internal bool UseDisabledComponent { get; set; }

        void OnEnable()
        {
            if (!IsEntityAlive()) return;

            if (UseDisabledComponent)
            {
                World.Remove<GameObjectDisabled>(EntityReference);
            }
        }

        void OnDisable()
        {
            if (!IsEntityAlive()) return;

            if (UseDisabledComponent)
            {
                World.Add<GameObjectDisabled>(EntityReference);
            }
        }

        void OnDestroy()
        {
            if (IsEntityAlive())
            {
                World.Destroy(EntityReference);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntityAlive()
        {
            if (World == null) return false;
            return World.IsAlive(EntityReference);
        }
    }
}