namespace MessageHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using System.Runtime.Caching;
    using System.Linq;

    public sealed class AppConnectedClientRepository<TConnectedClient> : IConnectedClientRepository<TConnectedClient> where TConnectedClient : IConnectedClient
    {
        /// <summary>
        /// The name of the cache
        /// </summary>
        private static string _cacheName = Guid.NewGuid().ToString();

        /// <summary>
        /// Backing lazy-initialized field for the cache
        /// </summary>
        private Lazy<MemoryCache> _cache = new Lazy<MemoryCache>(() => new MemoryCache(_cacheName));

        /// <summary>
        /// Gets a value for the cache used by this instance.
        /// </summary>
        private MemoryCache Cache
        {
            get
            {
                return _cache.Value;
            }
        }

        /// <summary>
        /// Gets the count of connected clients in this repository
        /// </summary>
        public long Count
        {
            get
            {
                return Cache.GetCount();
            }
        }

        /// <summary>
        /// Adds a single connected client to the repository
        /// </summary>
        /// <param name="client">The client to add</param>
        public void Add(TConnectedClient client)
        {
            Cache.Add(client.Id.ToString(), client, null);
        }

        /// <summary>
        /// Gets a list of all connected clients
        /// </summary>
        /// <returns>A list of all connected clients</returns>
        public IList<TConnectedClient> All()
        {
            return Cache.Select(kvp => (TConnectedClient)kvp.Value).ToList();
        }

        /// <summary>
        /// Removes a single connected client from the repository by client Id
        /// </summary>
        /// <param name="clientId">The client Id</param>
        public void Remove(Guid clientId)
        {
            Cache.Remove(clientId.ToString());
        }

        /// <summary>
        /// Gets a single connected client by Id
        /// </summary>
        /// <param name="clientId">The connected client's Id</param>
        /// <returns>A single instance of <typeparamref name="TConnectedClient"/></returns>
        public TConnectedClient Single(Guid clientId)
        {
            return (TConnectedClient)Cache.Get(clientId.ToString());
        }
    }
}
