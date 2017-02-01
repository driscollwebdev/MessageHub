using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Messaging;

namespace MessageHub.Msmq.Test
{
    [TestClass]
    public class MsmqConnectedClientTests
    {
        [TestMethod]
        public void QueueShouldExistAfterReceiveIsCalled()
        {
            using (MsmqConnectedClient testClient = new MsmqConnectedClient())
            {
                testClient.QueuePath = @".\private$\testClient";
                testClient.Receive(Guid.NewGuid(), Message.Create().WithData("hello"));

                Assert.IsTrue(MessageQueue.Exists(testClient.QueuePath));
            }
        }

        [TestMethod]
        public void MessageShouldBeInQueueAfterReceive()
        {
            using (MsmqConnectedClient testClient = new MsmqConnectedClient())
            {
                testClient.Id = Guid.NewGuid();
                testClient.QueuePath = @".\private$\" + testClient.Id.ToString();

                if (!MessageQueue.Exists(testClient.QueuePath))
                {
                    MessageQueue.Create(testClient.QueuePath);
                }

                testClient.Receive(Guid.NewGuid(), Message.Create().WithData("hello"));

                MessageQueue queue = new MessageQueue(@".\private$\" + testClient.Id.ToString());
                System.Messaging.Message[] messages = queue.GetAllMessages();
                Assert.IsTrue(messages.Length > 0);

                MessageQueue.Delete(testClient.QueuePath);
            }
        }
    }
}
