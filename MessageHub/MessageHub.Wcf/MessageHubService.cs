namespace MessageHub.Wcf
{
    using System;
    using Interfaces;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.ServiceModel;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public sealed class MessageHubService : IMessageHubService
    {
        private ConcurrentBag<ConnectedClient> _clients = new ConcurrentBag<ConnectedClient>();

        public void AddReceiver()
        {
            ConnectedClient client = new ConnectedClient();
            client.ClientCallback = OperationContext.Current.GetCallbackChannel<IMessageHubServiceReceiver>();
            client.ClientId = client.ClientCallback.Id;

            if (_clients.Any(c => c.ClientId == client.ClientId))
            {
                RemoveReceiver(client.ClientId);
            }

            _clients.Add(client);
        }

        public void RemoveReceiver(Guid receiverId)
        {
            ConnectedClient client = _clients.FirstOrDefault(c => c.ClientId == receiverId);

            if (client != null)
            {
                ConcurrentBag<ConnectedClient> newClients = new ConcurrentBag<ConnectedClient>(_clients);
                if (newClients.TryTake(out client))
                {
                    _clients = newClients;
                }
            }
        }

        public void Send(Guid fromHubId, Message message)
        {
            foreach (ConnectedClient client in _clients)
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
