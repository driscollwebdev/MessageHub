using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Interfaces;
using MessageHub.Wcf;
using System.ServiceModel;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class MessageHubServiceTests
    {
        private ServiceHost svc;
        private DuplexChannelFactory<IMessageHubService> svcChannel;
        private IMessageHubService client;
        private TestServiceReceiver test;

        [TestInitialize]
        public void Initialize()
        {
            svc = new ServiceHost(typeof(MessageHubService));
            svc.AddServiceEndpoint(typeof(IMessageHubService), new NetTcpBinding(), "net.tcp://localhost:8000/TestHubService");
            svc.Open();

            test = new TestServiceReceiver();

            svcChannel = new DuplexChannelFactory<IMessageHubService>(test, new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:8000/TestHubService"));
            client = svcChannel.CreateChannel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (svcChannel.State != System.ServiceModel.CommunicationState.Faulted)
                {
                    svcChannel.Close();
                }
            }
            catch
            {
                svcChannel.Abort();
            }

            svc.Close(TimeSpan.FromMilliseconds(100));
        }

        [TestMethod]
        public void ShouldHaveReceiverAfterAddReceiver()
        {
            client.AddReceiver();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ShouldNotHaveReceiverAfterRemoveReceiver()
        {
            client.RemoveReceiver(test.Id);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ShouldTriggerReceiveCallbackAfterSend()
        {
            string messageType = "test";
            string messageData = "Hello, world!";

            client.AddReceiver();
            // simulate sending from a different client.
            client.Send(Guid.NewGuid(), Message.Create().WithType(messageType).WithData(messageData));

            Assert.IsTrue(test.ReceiveCalled);
            Assert.AreEqual(messageType, test.RemoteMessage.Type);
            Assert.AreEqual(messageData, (string)test.RemoteMessage.GetDataObject());
        }

        [TestMethod]
        public void ShouldNotTriggerReceiveCallbackFromSameHubId()
        {
            string messageType = "test";
            string messageData = "Hello, world!";

            client.AddReceiver();
            client.Send(test.Id, Message.Create().WithType(messageType).WithData(messageData));

            Assert.IsFalse(test.ReceiveCalled);
            Assert.IsNull(test.RemoteMessage);
        }

        [TestMethod]
        public void ShouldNotTriggerReceiveCallbackIfNotAddedFirst()
        {
            string messageType = "test";
            string messageData = "Hello, world!";

            client.Send(test.Id, Message.Create().WithType(messageType).WithData(messageData));

            Assert.IsFalse(test.ReceiveCalled);
            Assert.IsNull(test.RemoteMessage);
        }

        private class TestServiceReceiver : IMessageHubServiceReceiver
        {
            public Guid Id { get; } = Guid.NewGuid();

            public bool ReceiveCalled { get; private set; }

            public Message RemoteMessage { get; private set; }

            public void Receive(Guid hubId, Message message)
            {
                ReceiveCalled = true;
                RemoteMessage = message;
            }
        }
    }
}
