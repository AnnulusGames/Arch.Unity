using UnityEngine;
using Arch.Core;

namespace Arch.Unity.Conversion
{
    [AddComponentMenu("Arch/Entity Converter")]
    [DefaultExecutionOrder(-100)]
    public sealed class EntityConverter : MonoBehaviour
    {
        [SerializeField] EntityConversionOptions options;

        World world;
        EntityReference entityReference;

        void Awake()
        {
            world = EntityConversion.DefaultWorld;
            entityReference = EntityConversion.Convert(gameObject, world, options);
        }

        void OnDestroy()
        {
            if (options.ConversionMode == ConversionMode.SyncWithEntity && IsEntityAlive())
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