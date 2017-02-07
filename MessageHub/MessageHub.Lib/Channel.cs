namespace MessageHub
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A class representing a specific channel/topic of interest
    /// </summary>
    public sealed class Channel
    {
        /// <summary>
        /// Lazy-initialized field for this channel's receivers
        /// </summary>
        private Lazy<IDictionary<string, IEnumerable<Receiver>>> _receivers = new Lazy<IDictionary<string, IEnumerable<Receiver>>>(() => new ConcurrentDictionary<string, IEnumerable<Receiver>>(), true);

        /// <summary>
        /// Gets a value for the channel's receivers
        /// </summary>
        private IDictionary<string, IEnumerable<Receiver>> Receivers
        {
            get
            {
                return _receivers.Value;
            }
        }

        /// <summary>
        /// An event that is fired when a message is sent from this channel.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSending;

        /// <summary>
        /// Gets the Id of this channel
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets a value for the name of this channel
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="Channel"/> class
        /// </summary>
        private Channel() { }

        /// <summary>
        /// Creates an instance of the <see cref="Channel"/> class
        /// </summary>
        /// <returns>A <see cref="Channel"/> instance</returns>
        public static Channel Create()
        {
            return new Channel();
        }

        /// <summary>
        /// Sets the name of the current instance and returns the instance
        /// </summary>
        /// <param name="name">The name of the current instance</param>
        /// <returns>The current instance</returns>
        public Channel WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the Id of the current instance and returns the instance
        /// </summary>
        /// <param name="id">The Id of the current instance</param>
        /// <returns>The current instance</returns>
        public Channel WithId(Guid id)
        {
            Id = id;
            return this;
        }

        public Task Send<TData>(string messageType, TData data)
        {
            Message message = CreateMessage(messageType).WithData(data);

            return OnMessageSending(message);
        }

        public Task Send<TData>(string messageType, TData data, SerializationType serializationType)
        {
            Message message = CreateMessage(messageType).WithData(data, serializationType);

            return OnMessageSending(message);
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

        internal Task Receive(Message message)
        {
            if (message.ChannelName != Name)
            {
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(message.Type))
            {
                return Task.CompletedTask;
            }

            var receiverActions = GetReceiverActions(message.Type);
            IList<Task> tasks = new List<Task>();

            foreach(var action in receiverActions)
            {
                var actionCopy = action;
                tasks.Add(actionCopy(message.GetDataObject()));
            }

            return Task.WhenAll(tasks); 
        }

        private Message CreateMessage(string messageType)
        {
            return Message.Create()
                          .FromChannelId(Id)
                          .ToChannelName(Name)
                          .WithType(messageType);
        }

        private Task OnMessageSending(Message message)
        {
            MessageSending(this, new MessageEventArgs(message));
            return Receive(message);
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
