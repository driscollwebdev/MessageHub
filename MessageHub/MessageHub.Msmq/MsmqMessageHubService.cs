namespace MessageHub.Msmq
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class MsmqMessageHubService : IMessageHubService, IDisposable
    {
        private IConnectedClientRepository<MsmqConnectedClient> _clients;
        private MessageQueue _queue;

        public MsmqMessageHubService(string queuePath, IConnectedClientRepository<MsmqConnectedClient> clients)
        {
            _clients = clients;

            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath);
            }

            _queue = new MessageQueue(queuePath);
            _queue.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            _queue.ReceiveCompleted += OnReceiveCompleted;

            _queue.BeginReceive();
        }

        private void OnReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message msg = mq.EndReceive(e.AsyncResult);

            try
            {
                MessageEnvelope env = (MessageEnvelope)msg.Body;

                switch (env.ServiceOp)
                {
                    case HubServiceOperation.Send:
                        MessageHub.Message hubMsg = (MessageHub.Message)env.Contents;
                        Send(env.SenderId, hubMsg);
                        break;
                    case HubServiceOperation.AddReceiver:
                        MsmqConnectedClient client = (MsmqConnectedClient)env.Contents;
                        AddReceiver(client);
                        break;
                    case HubServiceOperation.RemoveReceiver:
                        Guid clientId = (Guid)env.Contents;
                        RemoveReceiver(clientId);
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                msg.Dispose();
                mq.BeginReceive();
            }
        }

        void IMessageHubService.AddReceiver(Guid receiverId)
        {
            throw new NotImplementedException();
        }

        public void AddReceiver(MsmqConnectedClient client)
        {
            MsmqConnectedClient existing = _clients.Single(client.Id);

            if (existing != null)
            {
                _clients.Remove(existing.Id);
            }

            _clients.Add(client);
        }

        public void RemoveReceiver(Guid receiverId)
        {
            _clients.Remove(receiverId);
        }

        public void Send(Guid senderId, MessageHub.Message message)
        {
            var clients = _clients.All();

            foreach(MsmqConnectedClient client in clients)
            {
                if (client.Id == senderId)
                {
                    continue;
                }

                client.Receive(senderId, message);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _queue.ReceiveCompleted -= OnReceiveCompleted;
                    _queue.Purge();
                    _queue.Close();
                    _queue.Dispose();

                    MessageQueue.Delete(_queue.Path);
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
