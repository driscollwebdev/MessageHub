using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class MessageHubTests
    {
        [TestMethod]
        public void ShouldReturnAChannelInstance()
        {
            MessageHub hub = MessageHub.Create();
            Channel actual = hub.Channel("test");

            Assert.IsNotNull(actual);
            Assert.AreEqual("test", actual.Name);
        }
    }
}
