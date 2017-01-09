namespace MessageHub.Hubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;

    /// <summary>
    /// A message hub class for publishing and subscribing to local messages/events
    /// </summary>
    public sealed class LocalMessageHub : IMessageHub
    {
        /// <summary>
        /// A backing dictionary for the Channels property
        /// </summary>
        private Lazy<IDictionary<string, Channel>> _channels = new Lazy<IDictionary<string, Channel>>(() => new ConcurrentDictionary<string, Channel>(), true);

        /// <summary>
        /// Gets the value for the channels available through this hub.
        /// </summary>
        private IDictionary<string, Channel> Channels
        {
            get
            {
                return _channels.Value;
            }
        }

        /// <summary>
        /// A lock for the broadcasting event.
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// An event that is fired when a message is broadcast
        /// </summary>
        public event EventHandler<MessageEventArgs> Broadcasting;

        /// <summary>
        /// The unique identifier of this hub instance
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalMessageHub"/> class
        /// </summary>
        private LocalMessageHub() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalMessageHub"/> class
        /// </summary>
        /// <returns>An instance of <see cref="IMessageHub"/></returns>
        public static IMessageHub Create()
        {
            return new LocalMessageHub();
        }

        /// <summary>
        /// Gets or creates the channel indicated by name
        /// </summary>
        /// <param name="name">The name of the channel to retrieve</param>
        /// <returns>An instance of the <see cref="Channel"/> class</returns>
        public Channel Channel(string name)
        {
            if (!Channels.ContainsKey(name))
            {
                var channel = MessageHub.Channel.Create().WithName(name);
                channel.MessageSending += Channel_MessageSending;

                Channels[name] = channel;
            }

            return Channels[name];
        }

        /// <summary>
        /// Handles the message sending event from a child channel
        /// </summary>
        /// <param name="sender">The channel sending the message</param>
        /// <param name="e">The message args</param>
        private async void Channel_MessageSending(object sender, MessageEventArgs e)
        {
            Channel sendingChannel = sender as Channel;

            if (sendingChannel == null || !Channels.Values.Any(c => c.Id == sendingChannel.Id))
            {
                return;
            }

            await Broadcast(e.Message); 
        }

        /// <summary>
        /// Broadcasts the passed message to all receivers
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>A task for continuation purposes</returns>
        public Task Broadcast(Message message)
        {
            if (Broadcasting != null)
            {
                lock (locker)
                {
                    Broadcasting?.Invoke(this, new MessageEventArgs(message));
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Receive a message from another source.
        /// </summary>
        /// <param name="message">The message to receive</param>
        /// <returns>A task for continuation purposes</returns>
        public Task Receive(Message message)
        {
            if (Channels.ContainsKey(message.ChannelName))
            {
                return Channels[message.ChannelName].Receive(message);
            }

            return Task.CompletedTask;
        }
    }
}
