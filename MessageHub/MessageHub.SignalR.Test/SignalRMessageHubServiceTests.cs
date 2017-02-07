using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MessageHub.Interfaces;
using MessageHub.Repositories;

namespace MessageHub.SignalR.Test
{
    [TestClass]
    public class SignalRMessageHubServiceTests
    {
        [TestMethod]
        public void ShouldHaveReceiverAfterAddReceiver()
        {
            var mockRepo = new Mock<IConnectedClientRepository<HubConnectedClient>>();
            var hub = new SignalRMessageHubService(mockRepo.Object);
            var mockRequest = new Mock<IRequest>();
            hub.Context = new HubCallerContext(mockRequest.Object, "123");
            var mockGroups = new Mock<IGroupManager>();
            hub.Groups = mockGroups.Object;

            var receiverId = Guid.NewGuid();

            hub.AddReceiver(new ConnectedClientData(receiverId, null));
            mockGroups.Verify(m => m.Add(It.IsAny<string>(), "__receivers"), Times.Once);
        }

        [TestMethod]
        public void ShouldNotHaveReceiverAfterRemoveReceiver()
        {
            var mockRepo = new AppConnectedClientRepository<HubConnectedClient>();
            var hub = new SignalRMessageHubService(mockRepo);
            var mockRequest = new Mock<IRequest>();
            hub.Context = new HubCallerContext(mockRequest.Object, "123");
            var mockGroups = new Mock<IGroupManager>();
            hub.Groups = mockGroups.Object;

            Guid recGuid = Guid.NewGuid();

            hub.AddReceiver(new ConnectedClientData(recGuid, null));

            hub.RemoveReceiver(recGuid);

            mockGroups.Verify(m => m.Remove(It.IsAny<string>(), "__receivers"), Times.Once);
        }

        [TestMethod]
        public void ShouldTriggerReceiveCallbackAfterSend()
        {
            bool failed = false;

            try
            {
                var mockRepo = new Mock<IConnectedClientRepository<HubConnectedClient>>();
                var hub = new SignalRMessageHubService(mockRepo.Object);
                var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
                var all = new Mock<IMessageHubServiceReceiver>();
                hub.Clients = mockClients.Object;
                all.Setup(m => m.Receive(It.IsAny<Guid>(), It.IsAny<Message>())).Verifiable();
                mockClients.Setup(m => m.Group("__receivers", It.IsAny<string>())).Returns(all.Object);
                hub.Send(Guid.NewGuid(), Message.Create().WithData("Test"));
                all.VerifyAll();
            }
            catch (MockException)
            {
                failed = true;
            }

            Assert.IsFalse(failed);
        }

        //[TestMethod]
        //public void ShouldTriggerReceiveCallbackAfterSecureSend()
        //{
        //    bool failed = false;

        //    try
        //    {
        //        var mockRepo = new Mock<IConnectedClientRepository<HubConnectedClient>>();
        //        var hub = new SignalRMessageHubService(mockRepo.Object);
        //        var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
        //        var all = new Mock<IMessageHubServiceReceiver>();
        //        hub.Clients = mockClients.Object;
        //        all.Setup(m => m.Receive(It.IsAny<Guid>(), It.IsAny<SecureMessageContainer>())).Verifiable();
        //        mockClients.Setup(m => m.Group("__receivers", It.IsAny<string>())).Returns(all.Object);
        //        hub.Send(Guid.NewGuid(), Message.Create().WithData("Test"));
        //        all.VerifyAll();
        //    }
        //    catch (MockException)
        //    {
        //        failed = true;
        //    }

        //    Assert.IsFalse(failed);
        //}

        [TestMethod]
        public void ShouldNotTriggerReceiveCallbackFromSameHubId()
        {
            var mockRepo = new Mock<IConnectedClientRepository<HubConnectedClient>>();
            var hub = new SignalRMessageHubService(mockRepo.Object);
            var mockRequest = new Mock<IRequest>();
            hub.Context = new HubCallerContext(mockRequest.Object, "123");
            var mockGroups = new Mock<IGroupManager>();
            hub.Groups = mockGroups.Object;
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;

            var receiver = new Mock<IMessageHubServiceReceiver>();
            Guid excludedGuid = Guid.NewGuid();
            receiver.SetupGet(r => r.Id).Returns(excludedGuid);
            hub.AddReceiver(new ConnectedClientData(excludedGuid, null));
            hub.Send(excludedGuid, Message.Create().WithData("Test"));
            receiver.Verify(r => r.Receive(excludedGuid, It.IsAny<Message>()), Times.Never);
        }

        [TestMethod]
        public void ShouldNotTriggerReceiveCallbackIfNotAddedFirst()
        {
            var mockRepo = new Mock<IConnectedClientRepository<HubConnectedClient>>();
            var hub = new SignalRMessageHubService(mockRepo.Object);
            var mockRequest = new Mock<IRequest>();
            hub.Context = new HubCallerContext(mockRequest.Object, "123");
            var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            hub.Clients = mockClients.Object;

            var receiver = new Mock<IMessageHubServiceReceiver>();
            hub.Send(Guid.NewGuid(), Message.Create().WithData("Test"));
            receiver.Verify(r => r.Receive(It.IsAny<Guid>(), It.IsAny<Message>()), Times.Never);

        }
    }
}
