using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void ShouldHaveHubIdAfterWithHubId()
        {
            Guid hubId = Guid.NewGuid();
            var test = Channel.Create().WithHubId(hubId);

            Assert.IsNotNull(test);
            Assert.AreEqual(hubId, test.HubId);
        }
    }
}
