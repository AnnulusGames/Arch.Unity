using NUnit.Framework;

namespace Arch.Unity.Tests.Runtime
{
    public class EntityNameTest
    {
        [Test]
        public void Test_EntityName_Equals()
        {
            var nameA = new EntityName("Foo");
            var nameB = new EntityName("Foo");
            Assert.That(nameA, Is.EqualTo(nameB));
        }
    }
}