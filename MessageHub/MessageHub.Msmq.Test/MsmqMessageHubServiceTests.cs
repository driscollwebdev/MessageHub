using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Repositories;
using System.Messaging;

namespace MessageHub.Msmq.Test
{
    [TestClass]
    public class MsmqMessageHubServiceTests
    {
        [TestMethod]
        public void ServiceQueueShouldExistAfterConstructor()
        {
            string queuePath = @".\private$\testSvc";

            using (MsmqMessageHubService testSvc = new MsmqMessageHubService(queuePath, new AppConnectedClientRepository<MsmqConnectedClient>()))
            {
                Assert.IsTrue(MessageQueue.Exists(queuePath));
            }
        }

        [TestMethod]
        public void ClientShouldBeInRepoAfterAddReceiver()
        {
            var repo = new AppConnectedClientRepository<MsmqConnectedClient>();
            MsmqConnectedClient subject = new MsmqConnectedClient
            {
                Id = Guid.NewGuid()
            };

            using (MsmqMessageHubService testSvc = new MsmqMessageHubService(@".\private$\testSvc", repo))
            {
                testSvc.AddReceiver(subject);

                MsmqConnectedClient actual = repo.Single(subject.Id);
                Assert.IsNotNull(actual);
            }

            repo.Remove(subject.Id);
            subject.Dispose();
        }

        [TestMethod]
        public void ClientShouldNotBeInRepoAfterRemoveReceiver()
        {
            var repo = new AppConnectedClientRepository<MsmqConnectedClient>();
            MsmqConnectedClient subject = new MsmqConnectedClient
            {
                Id = Guid.NewGuid()
            };

            repo.Add(subject);

            using (MsmqMessageHubService testSvc = new MsmqMessageHubService(@".\private$\testSvc", repo))
            {
                testSvc.RemoveReceiver(subject.Id);

                MsmqConnectedClient actual = repo.Single(subject.Id);
                Assert.IsNull(actual);
            }

            subject.Dispose();
        }
    }
}
