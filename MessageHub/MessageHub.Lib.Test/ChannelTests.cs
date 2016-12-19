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
        public void ShouldHaveHubAfterWithHub()
        {
            IMessageHub hub = MessageHub.Create();
            var test = Channel.Create().WithHub(hub);

            Assert.IsNotNull(test);
            Assert.AreEqual(hub, test.Hub);
        }
    }
}
