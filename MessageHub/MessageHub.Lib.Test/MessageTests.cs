using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class MessageTests
    {
        [TestMethod]
        public void ShouldCreateInstanceOfType()
        {
            var test = Message.Create();

            Assert.IsNotNull(test);
        }

        [TestMethod]
        public void ShouldHaveTypeAfterWithType()
        {
            string type = "Test";
            var test = Message.Create().WithType(type);

            Assert.IsNotNull(test);
            Assert.AreEqual(type, test.Type);
        }

        [TestMethod]
        public void ShouldHaveChannelNameAfterWithChannelName()
        {
            string channelName = "Test";
            var test = Message.Create().ToChannelName(channelName);

            Assert.IsNotNull(test);
            Assert.AreEqual(channelName, test.ChannelName);
        }

        [TestMethod]
        public void ShouldHaveChannelIdAfterFromChannelId()
        {
            Guid channelId = Guid.NewGuid();
            var test = Message.Create().FromChannelId(channelId);

            Assert.IsNotNull(test);
            Assert.AreEqual(channelId, test.ChannelId);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndNoPassedType()
        {
            string data = "testData";
            var test = Message.Create().WithData(data);

            Assert.IsNotNull(test);
            Assert.AreEqual(data, (string)test.GetDataObject());
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndPassedType()
        {
            string data = "testData";
            var test = Message.Create().WithData(data);

            Assert.IsNotNull(test);
            Assert.AreEqual(data, (string)test.GetDataObject(typeof(string)));
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndGenericType()
        {
            string data = "testData";
            var test = Message.Create().WithData(data);

            Assert.IsNotNull(test);
            Assert.AreEqual(data, test.GetData<string>());
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndJsonSerializationWithAnonymousType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Json);

            Assert.IsNotNull(testMsg);
            dynamic actualData = testMsg.GetDataObject();
            Assert.AreEqual(data.IsTest, (bool)actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndJsonSerializationWithPassedType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Json);

            Assert.IsNotNull(testMsg);
            TestData actualData = (TestData)testMsg.GetDataObject(typeof(TestData));
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndJsonSerializationWithGenericType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Json);

            Assert.IsNotNull(testMsg);
            TestData actualData = testMsg.GetData<TestData>();
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndJsonSerializationWithPassedOtherType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Json);

            Assert.IsNotNull(testMsg);
            OtherTestData actualData = (OtherTestData)testMsg.GetDataObject(typeof(OtherTestData));
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndJsonSerializationWithGenericOtherType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Json);

            Assert.IsNotNull(testMsg);
            OtherTestData actualData = testMsg.GetData<OtherTestData>();
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldThrowWithXmlSerializationAndNoPassedType()
        {
            try
            {
                TestData data = new TestData { IsTest = true };
                Message testMsg = Message.Create().WithData(data, SerializationType.Xml);

                Assert.IsNotNull(testMsg);
                dynamic actualData = testMsg.GetDataObject();

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(NotSupportedException), ex.GetType());
            }
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndXmlSerializationWithPassedType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Xml);

            Assert.IsNotNull(testMsg);
            dynamic actualData = testMsg.GetDataObject(typeof(TestData));
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndXmlSerializationWithGenericType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Xml);

            Assert.IsNotNull(testMsg);
            TestData actualData = testMsg.GetData<TestData>();
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndXmlSerializationWithPassedOtherType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Xml);

            Assert.IsNotNull(testMsg);
            dynamic actualData = testMsg.GetDataObject(typeof(OtherTestData));
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        [TestMethod]
        public void ShouldHaveDataAfterWithDataAndXmlSerializationWithGenericOtherType()
        {
            TestData data = new TestData { IsTest = true };
            Message testMsg = Message.Create().WithData(data, SerializationType.Xml);

            Assert.IsNotNull(testMsg);
            OtherTestData actualData = testMsg.GetData<OtherTestData>();
            Assert.AreEqual(data.IsTest, actualData.IsTest);
        }

        public class TestData
        {
            public bool IsTest { get; set; }
        }

        [XmlRoot("TestData")]
        public class OtherTestData
        {
            public bool IsTest { get; set; }
        }
    }
}
