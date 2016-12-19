using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class MessageTests
    {
        [TestMethod]
        public void ShouldCreateInstanceOfType()
        {
            var test = Message.Create();

            Assert.IsNotNull(test);
        }

        [TestMethod]
        public void ShouldHaveTypeAfterWithType()
        {
            string type = "Test";
            var test = Message.Create().WithType(type);

            Assert.IsNotNull(test);
            Assert.AreEqual(type, test.Type);
        }

        [TestMethod]
        public void ShouldHaveChannelNameAfterWithChannelName()
        {
            string channelName = "Test";
            var test = Message.Create().WithChannelName(channelName);

            Assert.IsNotNull(test);
            Assert.AreEqual(channelName, test.ChannelName);
        }

        [TestMethod]
        public void ShouldHaveHubIdAfterWithHubId()
        {
            Guid hubId = Guid.NewGuid();
            var test = Message.Create().WithHubId(hubId);

            Assert.IsNotNull(test);
            Assert.AreEqual(hubId, test.HubId);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithData()
        {
            string data = "testData";
            var test = Message.Create().WithData(data);

            Assert.IsNotNull(test);
            Assert.AreEqual(data, (string)test.GetDataObject());
        }
    }
}
