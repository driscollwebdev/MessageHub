namespace MessageHub
{
    using Interfaces;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A class representing a specific channel/topic of interest
    /// </summary>
    public sealed class Channel
    {
        private Lazy<IDictionary<string, IEnumerable<Receiver>>> _receivers = new Lazy<IDictionary<string, IEnumerable<Receiver>>>(() => new ConcurrentDictionary<string, IEnumerable<Receiver>>(), true);

        private IDictionary<string, IEnumerable<Receiver>> Receivers
        {
            get
            {
                return _receivers.Value;
            }
        }

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

        public bool HasReceiver(string messageType, Guid receiverGuid)
        {
            if (Receivers.ContainsKey(messageType))
            {
                return Receivers[messageType].Any(r => r.Id == receiverGuid);
            }

            return false;
        }

        public Guid AddReceiver(string messageType, Func<object, Task> action)
        {
            Guid receiverGuid = Guid.NewGuid();
            AddReceiver(messageType, action, receiverGuid);

            return receiverGuid;
        }

        public void AddReceiver(string messageType, Func<object, Task> action, Guid receiverGuid)
        {
            ConcurrentBag<Receiver> recList = new ConcurrentBag<Receiver>();
            if (Receivers.ContainsKey(messageType))
            {
                recList = Receivers[messageType] as ConcurrentBag<Receiver>;
            }

            recList.Add(new Receiver { Id = receiverGuid, MessageType = messageType, OnMessageReceived = action });

            Receivers[messageType] = recList;
        }

        public bool RemoveReceiver(string messageType, Guid receiverGuid)
        {
            if (Receivers.ContainsKey(messageType))
            {
                Receiver receiver = Receivers[messageType].FirstOrDefault(r => r.Id == receiverGuid);
                if (receiver != null)
                {
                    ConcurrentBag<Receiver> newReceivers = new ConcurrentBag<Receiver>(Receivers[messageType]);
                    if (newReceivers.TryTake(out receiver))
                    {
                        Receivers[messageType] = newReceivers;
                        return true;
                    }
                }
            }

            return false;
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

            if (Receivers.ContainsKey(messageType))
            {
                instances = Receivers[messageType].Select(r => r.Id).ToList();
            }

            return instances;
        }

        private IList<Func<object, Task>> GetReceiverActions(string messageType)
        {
            IList<Func<object, Task>> actions = new List<Func<object, Task>>();

            if (Receivers.ContainsKey(messageType))
            {
                actions = Receivers[messageType].Select(r => r.OnMessageReceived).ToList();
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
