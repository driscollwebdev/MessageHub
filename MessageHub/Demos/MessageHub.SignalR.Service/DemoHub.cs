using MessageHub.Interfaces;
using System;

namespace MessageHub.SignalR.Service
{
    public class DemoHub : SignalRMessageHubService
    {
        public DemoHub(IConnectedClientRepository<HubConnectedClient> clients)
            : base(clients)
        {
        }
        protected override void OnConnectedClientAdded(HubConnectedClient client)
        {
            Console.WriteLine($"Client with Id {client.Id} joined on connection id {client.ConnectionId}.");
        }

        protected override void OnConnectedClientRemoved(HubConnectedClient client)
        {
            Console.WriteLine($"Client with Id {client.Id} disconnected.");
        }
    }
}
