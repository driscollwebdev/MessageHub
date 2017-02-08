namespace MessageHub.Wcf
{
    using System;
    using Interfaces;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Security;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public sealed class WcfMessageHubService : IMessageHubService
    {
        private IConnectedClientRepository<WcfConnectedClient> _clients;

        private Lazy<SecureKeyProvider> _keyProvider = new Lazy<SecureKeyProvider>(() => new SecureKeyProvider());

        private SecureKeyProvider KeyProvider
        {
            get
            {
                return _keyProvider.Value;
            }
        }

        public WcfMessageHubService(IConnectedClientRepository<WcfConnectedClient> clients)
        {
            _clients = clients;
        }

        public void AddReceiver(ConnectedClientData clientData)
        {
            WcfConnectedClient client = new WcfConnectedClient();
            client.ClientCallback = OperationContext.Current.GetCallbackChannel<IMessageHubServiceReceiver>();
            client.Id = client.ClientCallback.Id;
            client.PublicKey = clientData.PublicKey;

            WcfConnectedClient existing = _clients.Single(clientData.Id);

            if (existing != null)
            {
                RemoveReceiver(clientData.Id);
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

        public string GetServiceKey()
        {
            return KeyProvider.PublicKey;
        }

        public void Send(Guid senderId, Message message)
        {
            IList<WcfConnectedClient> receivers = _clients.All();

            foreach (WcfConnectedClient client in receivers)
            {
                try
                {
                    if (client.Id == senderId)
                    {
                        continue;
                    }

                    client.ClientCallback.Receive(senderId, message);
                }
                catch
                {
                    RemoveReceiver(client.Id);
                }
            }
        }

        public void SendSecure(Guid senderId, SecureMessageContainer secureMessage)
        {
            byte[] clearKey = KeyProvider.Decrypt(Convert.FromBase64String(secureMessage.EncryptedKey));
            byte[] clearIV = KeyProvider.Decrypt(Convert.FromBase64String(secureMessage.EncryptedIV));

            RSACryptoServiceProvider clientRsaProvider = new RSACryptoServiceProvider();

            IList<WcfConnectedClient> receivers = _clients.All();
            foreach (WcfConnectedClient client in receivers)
            {
                try
                {
                    if (client.Id == senderId)
                    {
                        continue;
                    }

                    clientRsaProvider.FromXmlString(client.PublicKey);

                    SecureMessageContainer clientMessage = new SecureMessageContainer();
                    clientMessage.EncryptedData = secureMessage.EncryptedData;
                    clientMessage.EncryptedKey = Convert.ToBase64String(clientRsaProvider.Encrypt(clearKey, false));
                    clientMessage.EncryptedIV = Convert.ToBase64String(clientRsaProvider.Encrypt(clearIV, false));

                    client.ClientCallback.Receive(senderId, clientMessage);
                }
                catch
                {
                    RemoveReceiver(client.Id);
                }
            }
        }
    }
}
