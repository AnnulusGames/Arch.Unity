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
        }
    }
}