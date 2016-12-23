using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Interfaces;
using System.Threading.Tasks;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class ChannelTests
    {
        [TestMethod]
        public void CreateShouldReturnChannelInstance()
        {
            var test = Channel.Create();

            Assert.IsNotNull(test);
            Assert.IsInstanceOfType(test, typeof(Channel));
        }

        [TestMethod]
        public void ShouldHaveNameAfterWithName()
        {
            string name = "test";
            var test = Channel.Create().WithName(name);

            Assert.IsNotNull(test);
            Assert.AreEqual(name, test.Name);
        }

        [TestMethod]
        public void ShouldHaveIdAfterWithId()
        {
            Guid id = Guid.NewGuid();
            var test = Channel.Create().WithId(id);

            Assert.IsNotNull(test);
            Assert.AreEqual(id, test.Id);
        }

        [TestMethod]
        public void ShouldHaveReceiverAfterAddReceiver()
        {
            var testChannel = Channel.Create();
            Guid testRecGuid = testChannel.AddReceiver("test", (obj) => { return Task.CompletedTask; });

            Assert.IsTrue(testChannel.HasReceiver("test", testRecGuid));
        }

        [TestMethod]
        public void ShouldNotHaveReceiverAfterRemoveReceiver()
        {
            var testChannel = Channel.Create();
            Guid testRecGuid = testChannel.AddReceiver("test", (obj) => { return Task.CompletedTask; });

            Assert.IsTrue(testChannel.HasReceiver("test", testRecGuid));

            bool isRemoved = testChannel.RemoveReceiver("test", testRecGuid);

            Assert.IsTrue(isRemoved);
            Assert.IsFalse(testChannel.HasReceiver("test", testRecGuid));
        }
    }
}
