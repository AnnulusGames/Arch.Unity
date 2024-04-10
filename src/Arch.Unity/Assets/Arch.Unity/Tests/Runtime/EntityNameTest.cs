using NUnit.Framework;

namespace Arch.Unity.Tests.Runtime
{
    public class EntityNameTest
    {
        [Test]
        public void Test_EntityName_Equals()
        {
            EntityName nameA = "Foo";
            EntityName nameB = "Foo";
            Assert.That(nameA, Is.EqualTo(nameB));
        }
    }
}