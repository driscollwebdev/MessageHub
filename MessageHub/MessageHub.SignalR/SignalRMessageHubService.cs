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

        private static Lazy<ConcurrentBag<ConnectedClient>> _clients = new Lazy<ConcurrentBag<ConnectedClient>>(() => new ConcurrentBag<ConnectedClient>(), true);

        private static ConcurrentBag<ConnectedClient> ConnectedClients
        {
            get
            {
                return _clients.Value;
            }

            set
            {
                _clients = new Lazy<ConcurrentBag<ConnectedClient>>(() => value, true);
            }
        }

        public void AddReceiver(Guid receiverId)
        {
            ConnectedClient client = new ConnectedClient();
            client.ConnectionId = Context.ConnectionId;
            client.ClientId = receiverId;

            if (ConnectedClients.Any(c => c.ClientId == client.ClientId))
            {
                RemoveReceiver(client.ClientId);
            }

            ConnectedClients.Add(client);
            Groups.Add(client.ConnectionId, ReceiverGroup);
        }

        public void RemoveReceiver(Guid receiverId)
        {
            ConnectedClient client = ConnectedClients.FirstOrDefault(c => c.ClientId == receiverId);

            if (client != null)
            {
                ConcurrentBag<ConnectedClient> newClients = new ConcurrentBag<ConnectedClient>(ConnectedClients);
                if (newClients.TryTake(out client))
                {
                    Groups.Remove(client.ConnectionId, ReceiverGroup);
                    ConnectedClients = newClients;
                }
            }
        }

        public void Send(Guid fromHubId, Message message)
        {
            ConnectedClient client = ConnectedClients.FirstOrDefault(c => c.ClientId == fromHubId);
            string excludedClient = string.Empty;

            if (client != null)
            {
                excludedClient = client.ConnectionId;
            }

            var group = Clients.Group(ReceiverGroup, excludedClient);
            if (group != null)
            {
                group.Receive(fromHubId, message);
            }
        }

        private class ConnectedClient
        {
            public Guid ClientId { get; set; }

            public string ConnectionId { get; set; }
        }
    }
}
