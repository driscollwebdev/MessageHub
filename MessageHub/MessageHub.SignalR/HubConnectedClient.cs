namespace MessageHub.SignalR
{
    using System;
    using Interfaces;

    public class HubConnectedClient : IConnectedClient
    {
        public Guid Id { get; set; }

        public string PublicKey { get; set; }

        public string ConnectionId { get; set; }
    }
}
