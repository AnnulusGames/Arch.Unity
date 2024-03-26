using System.Collections.Generic;
using UnityEngine;
using UnityComponent = UnityEngine.Component;
using Arch.Core;
using Arch.Core.Utils;
using System;

namespace Arch.Unity.Conversion
{
    public static class EntityConversion
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

#if !ARCH_UNITY_DISABLE_AUTOMATIC_BOOTSTRAP
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            var world = DefaultWorld = World.Create();

            Application.quitting += () =>
            {
                if (World.Worlds[world.Id] != null) world.Dispose();
            };
        }
#endif

        public static EntityReference Convert(GameObject gameObject, ConversionMode conversionMode = ConversionMode.ConvertAndDestroy, bool convertHybridComponents = false)
        {
            return Convert(gameObject, DefaultWorld, conversionMode, convertHybridComponents);
        }

        public static EntityReference Convert(GameObject gameObject, World world, ConversionMode conversionMode = ConversionMode.ConvertAndDestroy, bool convertHybridComponents = false)
        {
            var components = gameObject.GetComponents<UnityComponent>();
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

            world ??= DefaultWorld;
            var entityReference = world.Reference(converter.Convert(world));

            if (conversionMode == ConversionMode.ConvertAndDestroy)
            {
                UnityEngine.Object.Destroy(gameObject);
            }

            return entityReference;
        }

        public static World DefaultWorld { get; set; }
    }
}