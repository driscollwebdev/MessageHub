using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Lib.Interfaces;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class ChannelTests
    {
        [TestMethod]
        public void CreateShouldReturnChannelInstance()
        {
            var test = Channel.Create();

            Assert.IsNotNull(test);
            Assert.IsInstanceOfType(test, typeof(Channel));
        }

        [TestMethod]
        public void ShouldHaveNameAfterWithName()
        {
            string name = "test";
            var test = Channel.Create().WithName(name);

            Assert.IsNotNull(test);
            Assert.AreEqual(name, test.Name);
        }

        [TestMethod]
        public void ShouldHaveIdAfterWithId()
        {
            Guid id = Guid.NewGuid();
            var test = Channel.Create().WithId(id);

            Assert.IsNotNull(test);
            Assert.AreEqual(id, test.Id);
        }
    }
}
