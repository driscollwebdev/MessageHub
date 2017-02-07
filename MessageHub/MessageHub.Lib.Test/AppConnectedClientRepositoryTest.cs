using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageHub.Interfaces;
using MessageHub.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace MessageHub.Lib.Test
{
    [TestClass]
    public class AppConnectedClientRepositoryTest
    {
        [TestMethod]
        public void SingleShouldReturnClientAfterAdd()
        {
            AppConnectedClientRepository<TestConnectedClient> repo = new AppConnectedClientRepository<TestConnectedClient>();

            TestConnectedClient expected = new TestConnectedClient { OtherProp = "foobar" };

            repo.Add(expected);

            TestConnectedClient actual = repo.Single(expected.Id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.OtherProp, actual.OtherProp);
        }

        [TestMethod]
        public void AllShouldContainClientAfterAdd()
        {
            AppConnectedClientRepository<TestConnectedClient> repo = new AppConnectedClientRepository<TestConnectedClient>();

            TestConnectedClient expected = new TestConnectedClient { OtherProp = "foobar" };

            repo.Add(expected);

            IList<TestConnectedClient> actual = repo.All();

            Assert.IsTrue(actual.Any(c => c.Id == expected.Id));
        }

        [TestMethod]
        public void SingleShouldNotReturnClientAfterRemove()
        {
            AppConnectedClientRepository<TestConnectedClient> repo = new AppConnectedClientRepository<TestConnectedClient>();

            TestConnectedClient expected = new TestConnectedClient { OtherProp = "foobar" };

            repo.Add(expected);

            TestConnectedClient actual = repo.Single(expected.Id);

            Assert.IsNotNull(actual);

            repo.Remove(expected.Id);

            actual = repo.Single(expected.Id);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void AllShouldNotReturnClientAfterRemove()
        {
            AppConnectedClientRepository<TestConnectedClient> repo = new AppConnectedClientRepository<TestConnectedClient>();

            TestConnectedClient expected = new TestConnectedClient { OtherProp = "foobar" };

            repo.Add(expected);

            IList<TestConnectedClient> actual = repo.All();

            Assert.IsTrue(actual.Any(c => c.Id == expected.Id));

            repo.Remove(expected.Id);

            actual = repo.All();

            Assert.IsFalse(actual.Any(c => c.Id == expected.Id));
        }

        private class TestConnectedClient : IConnectedClient
        {
            public Guid Id { get; set; } = Guid.NewGuid();

            public string OtherProp { get; set; }

            public string PublicKey { get; set; }
        }
    }
}
