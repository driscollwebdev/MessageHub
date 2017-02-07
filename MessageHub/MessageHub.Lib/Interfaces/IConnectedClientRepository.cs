namespace MessageHub.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a repository for connected clients
    /// </summary>
    /// <typeparam name="TConnectedClient">The type of connected client</typeparam>
    public interface IConnectedClientRepository<TConnectedClient> where TConnectedClient : IConnectedClient
    {
        /// <summary>
        /// Gets a list of all connected clients
        /// </summary>
        /// <returns>A list of all connected clients</returns>
        IList<TConnectedClient> All();

        /// <summary>
        /// Gets a single connected client by Id
        /// </summary>
        /// <param name="clientId">The connected client's Id</param>
        /// <returns>A single instance of <typeparamref name="TConnectedClient"/></returns>
        TConnectedClient Single(Guid clientId);

        /// <summary>
        /// Adds a single connected client to the repository
        /// </summary>
        /// <param name="client">The client to add</param>
        void Add(TConnectedClient client);

        /// <summary>
        /// Removes a single connected client from the repository by client Id
        /// </summary>
        /// <param name="clientId">The client Id</param>
        void Remove(Guid clientId);

        /// <summary>
        /// Gets the count of connected clients in this repository
        /// </summary>
        long Count { get; }
    }
}
