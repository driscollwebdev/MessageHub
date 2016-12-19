using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class MessageTests
    {
        [TestMethod]
        public void ShouldCreateInstanceOfType()
        {
            var test = Message.Create<MessageStub>();

            Assert.IsNotNull(test);
            Assert.IsInstanceOfType(test, typeof(MessageStub));
        }

        [TestMethod]
        public void ShouldHaveTypeAfterWithType()
        {
            string type = "Test";
            var test = Message.Create<MessageStub>().WithType<MessageStub>(type);

            Assert.IsNotNull(test);
            Assert.AreEqual(type, test.Type);
        }

        private class MessageStub : Message
        {

        }
    }
}
