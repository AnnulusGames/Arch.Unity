using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Utils;
using UnityEngine;
using UnityComponent = UnityEngine.Component;

namespace Arch.Unity.Conversion
{
    [AddComponentMenu("Arch/Entity Converter")]
    [DefaultExecutionOrder(-100)]
    public sealed class EntityConverter : MonoBehaviour
    {
        sealed class Converter : IEntityConverter
        {
            readonly List<ComponentType> componentTypes = new();
            readonly List<object> components = new();

            public void AddComponent<T>(T component)
            {
                if (typeof(T).IsValueType)
                {
                    componentTypes.Add(typeof(T));
                }
                else
                {
                    componentTypes.Add(component.GetType());
                }

                components.Add(component); // TODO: avoid boxing
            }

            public Entity Convert(World world)
            {
                var entity = world.Create(componentTypes.ToArray());
                foreach (var component in components)
                {
                    world.Set(entity, component);
                }
                return entity;
            }
        }
        [SerializeField] ConversionMode conversionMode;
        [SerializeField] bool convertHybridComponents;

        World world;
        EntityReference entityReference;

        void Awake()
        {
            world = EntityConversion.DefaultWorld;

            var components = GetComponents<UnityComponent>();
            var converter = new Converter();

            converter.AddComponent(new GameObjectReference(gameObject));

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];

                if (component is IComponentConverter componentConverter)
                {
                    try
                    {
                        componentConverter.Convert(converter);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
                else if (conversionMode == ConversionMode.SyncWithEntity && convertHybridComponents)
                {
                    converter.AddComponent((object)component);
                }
            }

            entityReference = world.Reference(converter.Convert(world));

            if (conversionMode == ConversionMode.ConvertAndDestroy)
            {
                Destroy(gameObject);
            }
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
