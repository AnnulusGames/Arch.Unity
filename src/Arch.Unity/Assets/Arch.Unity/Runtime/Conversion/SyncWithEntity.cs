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

        void OnDestroy()
        {
            if (IsEntityAlive())
            {
                World.Destroy(EntityReference);
            }
        }

        public bool IsEntityAlive()
        {
            if (World == null) return false;
            return World.IsAlive(EntityReference);
        }
    }
}