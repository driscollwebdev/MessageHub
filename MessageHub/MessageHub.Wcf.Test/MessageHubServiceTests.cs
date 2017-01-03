using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Interfaces;
using MessageHub.Wcf;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml;

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
            svc = new ServiceHost(typeof(WcfMessageHubService));
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
            client.AddReceiver(Guid.NewGuid());
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

            client.AddReceiver(test.Id);
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

            client.AddReceiver(test.Id);
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

        [DataContract(Namespace = "")]
        private class TestServiceReceiver : MessageHubServiceReceiver
        {
            public bool ReceiveCalled { get; private set; }

            public Message RemoteMessage { get; private set; }

            [OperationContract]
            public override void Receive(Guid hubId, Message message)
            {
                ReceiveCalled = true;
                RemoteMessage = message;
            }
        }

        private class TestServiceReceiverResolver : DataContractResolver
        {
            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                throw new NotImplementedException();
            }

            public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
            {
                throw new NotImplementedException();
            }
        }
    }
}
