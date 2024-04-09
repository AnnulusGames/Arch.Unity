using Arch.Core;
using Arch.Unity.Conversion;
using NUnit.Framework;
using UnityEngine;

namespace Arch.Unity.Tests.Runtime
{
    public class ConversionTest
    {
        [Test]
        public void Test_OnConvert()
        {
            var obj = new GameObject("Target");
            var converted = false;

            void OnConvert(EntityReference entity, World world)
            {
                converted = true;
            }

            EntityConversion.OnConvert += OnConvert;
            EntityConversion.Convert(obj);
            EntityConversion.OnConvert -= OnConvert;

            Assert.IsTrue(converted);

            Object.Destroy(obj);
        }

        [Test]
        public void Test_TryGetEntity()
        {
            var obj = new GameObject("Target");
            var entityA = EntityConversion.Convert(obj, new EntityConversionOptions() { ConversionMode = ConversionMode.SyncWithEntity });
            var result = EntityConversion.TryGetEntity(obj, out var entityB);

            Assert.IsTrue(result);
            Assert.That(entityA, Is.EqualTo(entityB));

            Object.Destroy(obj);
        }

        [Test]
        public void Test_TryGetGameObject()
        {
            var objA = new GameObject("Target");
            var entity = EntityConversion.Convert(objA, new EntityConversionOptions() { ConversionMode = ConversionMode.SyncWithEntity });
            var result = EntityConversion.TryGetGameObject(entity, out var objB);

            Assert.IsTrue(result);
            Assert.That(objA, Is.EqualTo(objB));

            Object.Destroy(objA);
        }
    }
}