namespace MessageHub.SignalR
{
    using System;
    using Interfaces;
    using Microsoft.AspNet.SignalR;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Collections.Generic;

    public class SignalRMessageHubService : Hub, IMessageHubService
    {
        private const string ReceiverGroup = "__receivers";
        private static ConcurrentBag<ConnectedClient> _clients = new ConcurrentBag<ConnectedClient>();

        public void AddReceiver(IMessageHubServiceReceiver receiver)
        {
            ConnectedClient client = new ConnectedClient();
            client.ConnectionId = Context.ConnectionId;
            client.ClientId = receiver.Id;

            if (_clients.Any(c => c.ClientId == client.ClientId))
            {
                RemoveReceiver(client.ClientId);
            }

            _clients.Add(client);
            Groups.Add(client.ConnectionId, ReceiverGroup);
        }

        public void RemoveReceiver(Guid receiverId)
        {
            ConnectedClient client = _clients.FirstOrDefault(c => c.ClientId == receiverId);

            if (client != null)
            {
                ConcurrentBag<ConnectedClient> newClients = new ConcurrentBag<ConnectedClient>(_clients);
                if (newClients.TryTake(out client))
                {
                    Groups.Remove(client.ConnectionId, ReceiverGroup);
                    _clients = newClients;
                }
            }
        }

        public void Send(Guid fromHubId, Message message)
        {
            ConnectedClient client = _clients.FirstOrDefault(c => c.ClientId == fromHubId);
            string excludedClient = string.Empty;

            if (client != null)
            {
                excludedClient = client.ConnectionId;
            }

            Clients.Groups(new List<string> { { ReceiverGroup } }, excludedClient).Receive(fromHubId, message);
        }

        private class ConnectedClient
        {
            public Guid ClientId { get; set; }

            public string ConnectionId { get; set; }
        }
    }
}
