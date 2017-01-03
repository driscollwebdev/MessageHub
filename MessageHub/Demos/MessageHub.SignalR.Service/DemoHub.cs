using System;

namespace MessageHub.SignalR.Service
{
    public class DemoHub : SignalRMessageHubService
    {
        protected override void OnConnectedClientAdded(ConnectedClient client)
        {
            Console.WriteLine($"Client with Id {client.ClientId} joined on connection id {client.ConnectionId}.");
        }

        protected override void OnConnectedClientRemoved(ConnectedClient client)
        {
            Console.WriteLine($"Client with Id {client.ClientId} disconnected.");
        }
    }
}
