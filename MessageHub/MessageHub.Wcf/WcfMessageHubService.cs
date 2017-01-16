namespace MessageHub.Wcf
{
    using System;
    using Interfaces;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.ServiceModel;
    using System.Collections.Generic;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public sealed class WcfMessageHubService : IMessageHubService
    {
        private IConnectedClientRepository<WcfConnectedClient> _clients;

        public WcfMessageHubService(IConnectedClientRepository<WcfConnectedClient> clients)
        {
            _clients = clients;
        }

        public void AddReceiver(Guid receiverId)
        {
            WcfConnectedClient client = new WcfConnectedClient();
            client.ClientCallback = OperationContext.Current.GetCallbackChannel<IMessageHubServiceReceiver>();
            client.Id = client.ClientCallback.Id;

            WcfConnectedClient existing = _clients.Single(receiverId);

            if (existing != null)
            {
                RemoveReceiver(receiverId);
            }

            _clients.Add(client);
        }

        public void RemoveReceiver(Guid receiverId)
        {
            WcfConnectedClient client = _clients.Single(receiverId);

            if (client != null)
            {
                _clients.Remove(client.Id);
            }
        }

        public void Send(Guid fromHubId, Message message)
        {
            IList<WcfConnectedClient> receivers = _clients.All();

            foreach (WcfConnectedClient client in receivers)
            {
                try
                {
                    if (client.Id == fromHubId)
                    {
                        continue;
                    }

                    client.ClientCallback.Receive(fromHubId, message);
                }
                catch
                {
                    RemoveReceiver(client.Id);
                }
            }
        }
    }
}
