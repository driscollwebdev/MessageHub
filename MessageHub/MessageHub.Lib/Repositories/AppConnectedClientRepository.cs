namespace MessageHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using System.Runtime.Caching;
    using System.Linq;

    public sealed class AppConnectedClientRepository<TConnectedClient> : IConnectedClientRepository<TConnectedClient> where TConnectedClient : IConnectedClient
    {
        private static string _cacheName = Guid.NewGuid().ToString();

        private Lazy<MemoryCache> _cache = new Lazy<MemoryCache>(() => new MemoryCache(_cacheName));

        private MemoryCache Cache
        {
            get
            {
                return _cache.Value;
            }
        }

        public long Count
        {
            get
            {
                return Cache.GetCount();
            }
        }

        public void Add(TConnectedClient client)
        {
            Cache.Add(client.Id.ToString(), client, null);
        }

        public IList<TConnectedClient> All()
        {
            return Cache.Select(kvp => (TConnectedClient)kvp.Value).ToList();
        }

        public void Remove(Guid clientId)
        {
            Cache.Remove(clientId.ToString());
        }

        public TConnectedClient Single(Guid clientId)
        {
            return (TConnectedClient)Cache.Get(clientId.ToString());
        }
    }
}
