using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Interfaces;
using MessageHub.Hubs;
using System.Messaging;

namespace MessageHub.Msmq.Test
{
    [TestClass]
    public class MsmqMessageHubTests
    {
        [TestMethod]
        public void ShouldReturnCorrectHubTypeFromCreate()
        {
            IMessageHub test = MsmqMessageHub.Create();

            Assert.IsInstanceOfType(test, typeof(MsmqMessageHub));
        }

        [TestMethod]
        public void ShouldReturnCorrectHubTypeFromCreateWithInner()
        {
            IMessageHub test = MsmqMessageHub.Create((LocalMessageHub)LocalMessageHub.Create());

            Assert.IsInstanceOfType(test, typeof(MsmqMessageHub));
        }

        [TestMethod]
        public void ShouldHaveValuesConfiguredAfterWithConfiguration()
        {

            IRemoteMessageHub test = MsmqMessageHub.Create();
            
            test.Configure<MsmqHubConfiguration>(c =>
            {
                c.RemoteQueuePath = $".\\private$\\{test.Id}";
            });
        }

        [TestMethod]
        public void ShouldThrowExceptionIfRemoteQueueNameIsNotSet()
        {
            try
            {
                IRemoteMessageHub test = MsmqMessageHub.Create();
                test.Connect();

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException));
            }
        }

        [TestMethod]
        public void LocalQueueShouldExistAfterConnect()
        {
            IRemoteMessageHub test = MsmqMessageHub.Create();
            test.Configure<MsmqHubConfiguration>(c =>
            {
                c.RemoteQueuePath = $".\\private$\\{test.Id}";
            });

            test.Connect();

            Assert.IsTrue(MessageQueue.Exists($".\\private$\\{test.Id}"));
        }

        [TestMethod]
        public void LocalQueueShouldNotExistAfterDisconnect()
        {
            IRemoteMessageHub test = MsmqMessageHub.Create();
            test = test.WithConfiguration(new MsmqHubConfiguration
            {
                RemoteQueuePath = $".\\private$\\{test.Id}"
            });

            test.Connect();

            test.Disconnect();

            Assert.IsFalse(MessageQueue.Exists($".\\private$\\{test.Id}"));
        }
    }
}
