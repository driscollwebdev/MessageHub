using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Interfaces;
using System.Threading.Tasks;
using MessageHub.Hubs;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class LocalMessageHubTests
    {
        [TestMethod]
        public void ShouldReturnAChannelInstance()
        {
            IMessageHub hub = LocalMessageHub.Create();
            Channel actual = hub.Channel("test");

            Assert.IsNotNull(actual);
            Assert.AreEqual("test", actual.Name);
        }

        [TestMethod]
        public async Task ShouldNotifyListenerWhenMessageReceived()
        {
            TestListener target = new TestListener();
            IMessageHub hub = LocalMessageHub.Create();
            Channel testChannel = hub.Channel("test");
            testChannel.AddReceiver("testMessage", target.ReceivedCallback);

            Assert.IsFalse(target.WasCalled);

            await testChannel.Send("testMessage", "some data");

            Assert.IsTrue(target.WasCalled);
        }

        private class TestListener
        {
            public bool WasCalled { get; set; }

            public Task<bool> ReceivedCallback(object data)
            {
                WasCalled = true;
                return Task.FromResult(WasCalled);
            }
        }
    }
}
