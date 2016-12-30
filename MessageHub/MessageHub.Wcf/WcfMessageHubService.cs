﻿namespace MessageHub.Wcf
{
    using System;
    using Interfaces;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.ServiceModel;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public sealed class WcfMessageHubService : IMessageHubService
    {
        private Lazy<ConcurrentBag<ConnectedClient>> _clients = new Lazy<ConcurrentBag<ConnectedClient>>(() => new ConcurrentBag<ConnectedClient>(), true);

        private ConcurrentBag<ConnectedClient> ConnectedClients
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

        public void AddReceiver(IMessageHubServiceReceiver receiver)
        {
            ConnectedClient client = new ConnectedClient();
            client.ClientCallback = OperationContext.Current.GetCallbackChannel<IMessageHubServiceReceiver>();
            client.ClientId = client.ClientCallback.Id;

            if (ConnectedClients.Any(c => c.ClientId == client.ClientId))
            {
                RemoveReceiver(client.ClientId);
            }

            ConnectedClients.Add(client);
        }

        public void RemoveReceiver(Guid receiverId)
        {
            ConnectedClient client = ConnectedClients.FirstOrDefault(c => c.ClientId == receiverId);

            if (client != null)
            {
                ConcurrentBag<ConnectedClient> newClients = new ConcurrentBag<ConnectedClient>(ConnectedClients);
                if (newClients.TryTake(out client))
                {
                    ConnectedClients = newClients;
                }
            }
        }

        public void Send(Guid fromHubId, Message message)
        {
            foreach (ConnectedClient client in ConnectedClients)
            {
                if (client.ClientId == fromHubId)
                {
                    continue;
                }

                client.ClientCallback.Receive(fromHubId, message);
            }
        }

        private class ConnectedClient
        {
            public Guid ClientId { get; set; }

            public IMessageHubServiceReceiver ClientCallback { get; set; }
        }
    }
}