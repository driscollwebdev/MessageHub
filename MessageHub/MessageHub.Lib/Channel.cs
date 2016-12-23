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
    public class Channel
    {
        private IDictionary<string, IEnumerable<Receiver>> _receivers = new ConcurrentDictionary<string, IEnumerable<Receiver>>();

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
            if (_receivers.ContainsKey(messageType))
            {
                return _receivers[messageType].Any(r => r.Id == receiverGuid);
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
            if (_receivers.ContainsKey(messageType))
            {
                recList = _receivers[messageType] as ConcurrentBag<Receiver>;
            }

            recList.Add(new Receiver { Id = receiverGuid, MessageType = messageType, OnMessageReceived = action });

            _receivers[messageType] = recList;
        }

        public bool RemoveReceiver(string messageType, Guid receiverGuid)
        {
            if (_receivers.ContainsKey(messageType))
            {
                Receiver receiver = _receivers[messageType].FirstOrDefault(r => r.Id == receiverGuid);
                if (receiver != null)
                {
                    ConcurrentBag<Receiver> newReceivers = new ConcurrentBag<Receiver>(_receivers[messageType]);
                    if (newReceivers.TryTake(out receiver))
                    {
                        _receivers[messageType] = newReceivers;
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
