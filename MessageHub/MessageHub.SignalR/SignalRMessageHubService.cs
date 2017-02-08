namespace MessageHub.SignalR
{
    using System;
    using Interfaces;
    using Microsoft.AspNet.SignalR;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Collections.Generic;
    using Security;
    using System.Security.Cryptography;

    public class SignalRMessageHubService : Hub, IMessageHubService
    {
        private const string ReceiverGroup = "__receivers";

        private IConnectedClientRepository<HubConnectedClient> _clients;

        private Lazy<SecureKeyProvider> _keyProvider = new Lazy<SecureKeyProvider>(() => new SecureKeyProvider());

        private SecureKeyProvider KeyProvider
        {
            get
            {
                return _keyProvider.Value;
            }
        }

        public SignalRMessageHubService(IConnectedClientRepository<HubConnectedClient> clients)
        {
            _clients = clients;
        }

        public void AddReceiver(ConnectedClientData clientData)
        {
            HubConnectedClient client = new HubConnectedClient();
            client.ConnectionId = Context.ConnectionId;
            client.Id = clientData.Id;
            client.PublicKey = clientData.PublicKey;

            HubConnectedClient existing = _clients.Single(client.Id);

            if (existing != null)
            {
                Groups.Remove(existing.ConnectionId, ReceiverGroup);
                _clients.Remove(client.Id);
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

        public string GetServiceKey()
        {
            return KeyProvider.PublicKey;
        }

        protected virtual void OnConnectedClientRemoved(HubConnectedClient client)
        {
        }

        public void Send(Guid senderId, Message message)
        {
            HubConnectedClient client = _clients.Single(senderId);
            string excludedClient = string.Empty;

            if (client != null)
            {
                excludedClient = client.ConnectionId;
            }

            var group = Clients.Group(ReceiverGroup, excludedClient);
            if (group != null)
            {
                group.Receive(senderId, message);
            }
        }

        public void SendSecure(Guid senderId, SecureMessageContainer secureMessage)
        {
            IList<HubConnectedClient> allClients = _clients.All();

            byte[] clearKey = KeyProvider.Decrypt(Convert.FromBase64String(secureMessage.EncryptedKey));
            byte[] clearIV = KeyProvider.Decrypt(Convert.FromBase64String(secureMessage.EncryptedIV));

            RSACryptoServiceProvider clientRsaProvider = new RSACryptoServiceProvider();

            foreach (HubConnectedClient client in allClients)
            {
                if (client.Id == senderId)
                {
                    continue;
                }

                var signalrClient = Clients.Client(client.ConnectionId);

                if (signalrClient != null)
                {
                    clientRsaProvider.FromXmlString(client.PublicKey);

                    SecureMessageContainer clientMessage = new SecureMessageContainer();
                    clientMessage.EncryptedData = secureMessage.EncryptedData;
                    clientMessage.EncryptedKey = Convert.ToBase64String(clientRsaProvider.Encrypt(clearKey, false));
                    clientMessage.EncryptedIV = Convert.ToBase64String(clientRsaProvider.Encrypt(clearIV, false));

                    signalrClient.ReceiveSecure(senderId, clientMessage);
                }
            }
        }

        protected virtual void OnSend(Guid senderId, Message message)
        {
        }
    }
}
