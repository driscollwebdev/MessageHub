namespace MessageHub.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IConnectedClientRepository<TConnectedClient> where TConnectedClient : IConnectedClient
    {
        IList<TConnectedClient> All();

        TConnectedClient Single(Guid clientId);

        void Add(TConnectedClient client);

        void Remove(Guid clientId);

        long Count { get; }
    }
}
