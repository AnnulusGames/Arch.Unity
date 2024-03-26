using UnityEngine;
using Arch.Core;

namespace Arch.Unity.Conversion
{
    [AddComponentMenu("Arch/Entity Converter")]
    [DefaultExecutionOrder(-100)]
    public sealed class EntityConverter : MonoBehaviour
    {
        [SerializeField] ConversionMode conversionMode;
        [SerializeField] bool convertHybridComponents;

        World world;
        EntityReference entityReference;

        void Awake()
        {
            world = EntityConversion.DefaultWorld;
            entityReference = EntityConversion.Convert(gameObject, world, conversionMode, convertHybridComponents);
        }

        void OnDestroy()
        {
            if (conversionMode == ConversionMode.SyncWithEntity && IsEntityAlive())
            {
                world.Destroy(entityReference);
            }
        }

        public bool IsEntityAlive()
        {
            if (world == null) return false;
            return world.IsAlive(entityReference);
        }
    }
}