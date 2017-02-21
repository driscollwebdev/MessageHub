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
        /// Gets a value for the default serialization type used for messages in this channel
        /// </summary>
        public SerializationType DefaultSerializationType { get; private set; } = SerializationType.None;

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

        /// <summary>
        /// Sets the default serialization type of the current instance
        /// </summary>
        /// <param name="serializationType">The serialization type</param>
        /// <returns>The current instance</returns>
        public Channel WithSerializationType(SerializationType serializationType)
        {
            DefaultSerializationType = serializationType;
            return this;
        }

        /// <summary>
        /// Send a message of the specified type with the specified data
        /// </summary>
        /// <typeparam name="TData">The type of data for the message payload</typeparam>
        /// <param name="messageType">The message type</param>
        /// <param name="data">The message payload</param>
        /// <returns>A task for continuation purposes</returns>
        public Task Send<TData>(string messageType, TData data)
        {
            return Send(messageType, data, DefaultSerializationType);
        }

        /// <summary>
        /// Sends a message of the specified type with the specified data serialized with the specified serializer type
        /// </summary>
        /// <typeparam name="TData">The type of data for the message payload</typeparam>
        /// <param name="messageType">The message type</param>
        /// <param name="data">The message payload</param>
        /// <param name="serializationType">The serialization type to use for serializing the payload</param>
        /// <returns>A task for continuation purposes</returns>
        public Task Send<TData>(string messageType, TData data, SerializationType serializationType)
        {
            Message message = CreateMessage(messageType).WithData(data, serializationType);

            return OnMessageSending(message);
        }

        /// <summary>
        /// Gets a value indicating whether there exists a receiver identified by the receiverGuid for the specified message type 
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="receiverGuid">The receiver Guid</param>
        /// <returns>[True] if the receiver exists for the message type, [False] otherwise</returns>
        public bool HasReceiver(string messageType, Guid receiverGuid)
        {
            if (Receivers.ContainsKey(messageType))
            {
                return Receivers[messageType].Any(r => r.Id == receiverGuid);
            }

            return false;
        }

        /// <summary>
        /// Adds a receiver to the channel for the specified message type and returns a unique ID for the receiver
        /// </summary>
        /// <param name="messageType">The message type that the receiver will listen for</param>
        /// <param name="action">The action to execute when a message of the specified type is received</param>
        /// <returns>The receiver's unique identifier</returns>
        public Guid AddReceiver(string messageType, Func<object, Task> action)
        {
            Guid receiverGuid = Guid.NewGuid();
            AddReceiver(messageType, action, receiverGuid);

            return receiverGuid;
        }

        /// <summary>
        /// Adds a receiver with the specified unique identifier to the channel for the specified message type
        /// </summary>
        /// <param name="messageType">The message type that the receiver will listen for</param>
        /// <param name="action">The action to execute when a message of the specified type is received</param>
        /// <param name="receiverGuid">The receiver's unique identifier</param>
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

        /// <summary>
        /// Removes the receiver identified by Id from notifications for the specified message type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <param name="receiverGuid">The receiver Id</param>
        /// <returns>[True] if the receiver was found and removed, [False] otherwise</returns>
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

        /// <summary>
        /// Triggers the channel to receive and propagate the message
        /// </summary>
        /// <param name="message">The received message</param>
        /// <returns>A task for continuation purposes</returns>
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

        /// <summary>
        /// Creates a message of the specified type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <returns>The message</returns>
        private Message CreateMessage(string messageType)
        {
            return Message.Create()
                          .FromChannelId(Id)
                          .ToChannelName(Name)
                          .WithType(messageType);
        }

        /// <summary>
        /// Triggers the MessageSending event when a message is sent to this channel
        /// </summary>
        /// <param name="message">The message sent to the channel</param>
        /// <returns>A task for continuation purposes</returns>
        private Task OnMessageSending(Message message)
        {
            MessageSending(this, new MessageEventArgs(message));
            return Receive(message);
        }

        /// <summary>
        /// Gets the actions to execute for the specified message type
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        private IList<Func<object, Task>> GetReceiverActions(string messageType)
        {
            IList<Func<object, Task>> actions = new List<Func<object, Task>>();

            if (Receivers.ContainsKey(messageType))
            {
                actions = Receivers[messageType].Select(r => r.OnMessageReceived).ToList();
            }

            return actions;
        }

        /// <summary>
        /// A private class used by Channel to represent a message receiver
        /// </summary>
        private class Receiver
        {
            /// <summary>
            /// Gets or sets a value for the Id of this receiver
            /// </summary>
            public Guid Id { get; set; } = Guid.NewGuid();

            /// <summary>
            /// Gets or sets a value for the message type to receive
            /// </summary>
            public string MessageType { get; set; }

            /// <summary>
            /// Gets or sets a value for the delegate to execute when a message is received
            /// </summary>
            public Func<object, Task> OnMessageReceived { get; set; }
        }
    }
}
