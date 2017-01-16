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

        private IConnectedClientRepository<HubConnectedClient> _clients;

        public SignalRMessageHubService(IConnectedClientRepository<HubConnectedClient> clients)
        {
            _clients = clients;
        }

        public void AddReceiver(Guid receiverId)
        {
            HubConnectedClient client = new HubConnectedClient();
            client.ConnectionId = Context.ConnectionId;
            client.Id = receiverId;

            HubConnectedClient existing = _clients.Single(receiverId);

            if (existing != null)
            {
                Groups.Remove(existing.ConnectionId, ReceiverGroup);
                _clients.Remove(receiverId);
            }

            _clients.Add(client);
            Groups.Add(client.ConnectionId, ReceiverGroup);

            OnConnectedClientAdded(client);
        }

        protected virtual void OnConnectedClientAdded(HubConnectedClient client)
        {
        }

        public void RemoveReceiver(Guid receiverId)
        {
            HubConnectedClient client = _clients.Single(receiverId);

            if (client != null)
            {
                Groups.Remove(client.ConnectionId, ReceiverGroup);
                _clients.Remove(client.Id);
            }

            OnConnectedClientRemoved(client);
        }

        protected virtual void OnConnectedClientRemoved(HubConnectedClient client)
        {
        }

        public void Send(Guid fromHubId, Message message)
        {
            HubConnectedClient client = _clients.Single(fromHubId);
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

        protected virtual void OnSend(Guid senderId, Message message)
        {
        }
    }
}
