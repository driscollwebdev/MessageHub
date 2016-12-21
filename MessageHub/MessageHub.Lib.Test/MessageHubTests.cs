using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Lib.Interfaces;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class MessageHubTests
    {
        [TestMethod]
        public void ShouldReturnAChannelInstance()
        {
            IMessageHub hub = MessageHub.Create();
            Channel actual = hub.Channel("test");

            Assert.IsNotNull(actual);
            Assert.AreEqual("test", actual.Name);
        }
    }
}
