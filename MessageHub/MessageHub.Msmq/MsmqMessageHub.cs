namespace MessageHub.Msmq
{
    using Hubs;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class MsmqMessageHub : RemoteMessageHub, IMessageHub, IMessageHubServiceReceiver
    {
        private string _localQueueName;

        private string _localQueuePath;

        private MessageQueue _localQueue;

        private string _remoteQueuePath;

        private MessageQueue _remoteQueue;

        private MsmqMessageHub() : base() { }

        private MsmqMessageHub(LocalMessageHub inner) : base(inner) { }

        public override Task Broadcast(MessageHub.Message message)
        {
            MessageEnvelope env = new MessageEnvelope();
            env.SenderId = Id;
            env.ServiceOp = HubServiceOperation.Send;
            env.Contents = message;

            Message msg = new Message();
            msg.Body = env;
            msg.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            _remoteQueue.Send(msg);
            return Task.CompletedTask;
        }

        public override Task Broadcast(SecureMessageContainer secureMessage)
        {
            MessageEnvelope env = new MessageEnvelope();
            env.SenderId = Id;
            env.ServiceOp = HubServiceOperation.Send;
            env.Contents = secureMessage;
            env.IsSecure = true;

            Message msg = new Message();
            msg.Body = env;
            msg.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            _remoteQueue.Send(msg);
            return Task.CompletedTask;
        }

        public void Receive(Guid senderId, MessageHub.Message message)
        {
            base.Receive(message);
        }

        public void Receive(Guid senderId, SecureMessageContainer secureMessage)
        {
            base.Receive(secureMessage);
        }

        public static IRemoteMessageHub Create()
        {
            return new MsmqMessageHub();
        }

        public static IRemoteMessageHub Create(LocalMessageHub inner)
        {
            return new MsmqMessageHub(inner);
        }

        public override IRemoteMessageHub WithConfiguration(IHubConfiguration config)
        {
            UseEncryption = config.UseEncryption;

            MsmqHubConfiguration msmqConfig = (MsmqHubConfiguration)config;

            _localQueueName = msmqConfig.LocalQueueName;
            _remoteQueuePath = msmqConfig.RemoteQueuePath;

            return this;
        }

        public override Task Connect()
        {
            if (_remoteQueue != null && _remoteQueue.CanWrite)
            {
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(_localQueueName))
            {
                _localQueueName = Id.ToString();
            }

            if (string.IsNullOrWhiteSpace(_remoteQueuePath))
            {
                throw new InvalidOperationException("You must set the remote queue path by calling WithRemoteQueuePath first.");
            }

            _localQueuePath = $".\\private$\\{_localQueueName}";

            if (!MessageQueue.Exists(_localQueuePath))
            {
                MessageQueue.Create(_localQueuePath);
            }

            _localQueue = new MessageQueue(_localQueuePath);
            _localQueue.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            _localQueue.ReceiveCompleted += OnReceiveCompleted;
            _localQueue.BeginReceive();

            _remoteQueue = new MessageQueue(_remoteQueuePath);
            _remoteQueue.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            SendConnectMessage();

            return Task.CompletedTask;
        }

        public override void Disconnect()
        {
            SendDisconnectMessage();

            _localQueue.ReceiveCompleted -= OnReceiveCompleted;
            _localQueue.Purge();
            _localQueue.Close();
            _localQueue.Dispose();

            MessageQueue.Delete(_localQueuePath);

            _remoteQueue.Close();
            _remoteQueue.Dispose();
        }

        private void OnReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;
            Message msg = mq.EndReceive(e.AsyncResult);

            MessageEnvelope env = (MessageEnvelope)msg.Body;

            if (env.IsSecure)
            {
                Receive(env.SenderId, (MessageHub.SecureMessageContainer)env.Contents);
            }
            else
            {
                Receive(env.SenderId, (MessageHub.Message)env.Contents);
            }

            msg.Dispose();
            mq.BeginReceive();
        }

        private void SendConnectMessage()
        {
            MsmqConnectedClient client = new MsmqConnectedClient
            {
                Id = this.Id,
                QueuePath = $"FormatName:DIRECT=OS:{Environment.MachineName}\\private$\\{_localQueueName}",
                PublicKey = this.PublicKey
            };

            MessageEnvelope env = new MessageEnvelope();
            env.SenderId = Id;
            env.ServiceOp = HubServiceOperation.AddReceiver;
            env.Contents = client;

            Message msg = new Message(env);
            msg.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            _remoteQueue.Send(msg);

            msg.Dispose();
        }

        private void SendDisconnectMessage()
        {
            MessageEnvelope env = new MessageEnvelope();
            env.SenderId = Id;
            env.ServiceOp = HubServiceOperation.RemoveReceiver;
            env.Contents = Id;

            Message msg = new Message(env);
            msg.Formatter = new XmlMessageFormatter(new Type[1] { typeof(MessageEnvelope) });

            _remoteQueue.Send(msg);

            msg.Dispose();
        }
    }
}
