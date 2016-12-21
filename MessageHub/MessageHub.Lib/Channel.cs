namespace MessageHub.Lib
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A class representing a specific channel/topic of interest
    /// </summary>
    public class Channel
    {
        private IDictionary<string, IList<Receiver>> _receivers = new Dictionary<string, IList<Receiver>>();

        public event EventHandler<MessageEventArgs> MessageSending;

        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Name { get; set; }

        private Channel() { }

        public static Channel Create()
        {
            return new Channel();
        }

        public Channel WithName(string name)
        {
            Name = name;
            return this;
        }

        public Channel WithId(Guid id)
        {
            Id = id;
            return this;
        }

        public async Task Send<TData>(string messageType, TData data)
        {
            Message message = Message.Create()
                                     .FromChannelId(Id)
                                     .ToChannelName(Name)
                                     .WithType(messageType)
                                     .WithData(data);

            await OnMessageSending(message);
        }

        public Guid AddReceiver(string messageType, Func<object, Task> action)
        {
            Guid receiverGuid = Guid.NewGuid();
            AddReceiver(messageType, action, receiverGuid);

            return receiverGuid;
        }

        public void AddReceiver(string messageType, Func<object, Task> action, Guid receiverGuid)
        {
            IList<Receiver> recList = new List<Receiver>();
            if (_receivers.ContainsKey(messageType))
            {
                recList = _receivers[messageType];
            }

            recList.Add(new Receiver { Id = receiverGuid, MessageType = messageType, OnMessageReceived = action });

            _receivers[messageType] = recList;
        }

        public bool RemoveReceiver(string messageType, Guid receiverGuid)
        {
            throw new NotImplementedException();
        }

        internal async Task Receive(Message message)
        {
            if (message.ChannelName != Name)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Type))
            {
                return;
            }

            var receiverActions = GetReceiverActions(message.Type);
            IList<Task> tasks = new List<Task>();

            foreach(var action in receiverActions)
            {
                var actionCopy = action;
                tasks.Add(actionCopy(message.GetDataObject()));
            }

            await Task.WhenAll(tasks); 
        }

        private async Task OnMessageSending(Message message)
        {
            MessageSending(this, new MessageEventArgs(message));
            await Receive(message);
        }

        private IList<Guid> GetReceiverIds(string messageType)
        {
            IList<Guid> instances = new List<Guid>();

            if (_receivers.ContainsKey(messageType))
            {
                instances = _receivers[messageType].Select(r => r.Id).ToList();
            }

            return instances;
        }

        private IList<Func<object, Task>> GetReceiverActions(string messageType)
        {
            IList<Func<object, Task>> actions = new List<Func<object, Task>>();

            if (_receivers.ContainsKey(messageType))
            {
                actions = _receivers[messageType].Select(r => r.OnMessageReceived).ToList();
            }

            return actions;
        }

        private class Receiver
        {
            public Guid Id { get; set; } = Guid.NewGuid();

            public string MessageType { get; set; }

            public Func<object, Task> OnMessageReceived { get; set; }
        }
    }
}
