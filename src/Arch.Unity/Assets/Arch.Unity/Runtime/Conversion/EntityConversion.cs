using System;
using System.Collections.Generic;
using UnityEngine;
using UnityComponent = UnityEngine.Component;
using Arch.Core;
using Arch.Core.Utils;

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

        public static EntityReference Convert(GameObject gameObject)
        {
            return Convert(gameObject, DefaultWorld, EntityConversionOptions.Default);
        }

        public static EntityReference Convert(GameObject gameObject, EntityConversionOptions options)
        {
            return Convert(gameObject, DefaultWorld, options);
        }

        public static EntityReference Convert(GameObject gameObject, World world)
        {
            return Convert(gameObject, world, EntityConversionOptions.Default);
        }

        public static EntityReference Convert(GameObject gameObject, World world, EntityConversionOptions options)
        {
            var components = gameObject.GetComponents<UnityComponent>();
            var converter = new Converter();

            converter.AddComponent(new GameObjectReference(gameObject));

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];

                if (component is SyncWithEntity)
                {
                    throw new InvalidOperationException("A GameObject that has already been synchronized with an entity cannot be converted again.");
                }

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
                else if (options.ConversionMode == ConversionMode.SyncWithEntity && options.ConvertHybridComponents)
                {
                    converter.AddComponent((object)component);
                }
            }

            world ??= DefaultWorld;
            var entityReference = world.Reference(converter.Convert(world));

            if (options.ConversionMode == ConversionMode.ConvertAndDestroy)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                var syncWithEntity = gameObject.AddComponent<SyncWithEntity>();
                syncWithEntity.World = world;
                syncWithEntity.EntityReference = entityReference;
            }

            return entityReference;
        }

        public static World DefaultWorld { get; set; }
    }
}