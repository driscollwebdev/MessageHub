namespace MessageHub.Hubs
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    public sealed class LocalMessageHub : IMessageHub
    {
        private Lazy<IDictionary<string, Channel>> _channels = new Lazy<IDictionary<string, Channel>>(() => new ConcurrentDictionary<string, Channel>(), true);

        private IDictionary<string, Channel> Channels
        {
            get
            {
                return _channels.Value;
            }
        }

        private object locker = new object();

        public event EventHandler<MessageEventArgs> Broadcasting;

        public Guid Id { get; } = Guid.NewGuid();

        private LocalMessageHub() { }

        public static IMessageHub Create()
        {
            return new LocalMessageHub();
        }

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

        private async void Channel_MessageSending(object sender, MessageEventArgs e)
        {
            Channel sendingChannel = sender as Channel;

            if (sendingChannel == null || !Channels.Values.Any(c => c.Id == sendingChannel.Id))
            {
                return;
            }

            await Broadcast(e.Message); 
        }

        public Task Broadcast(Message message)
        {
            lock (locker)
            {
                Broadcasting(this, new MessageEventArgs(message));
            }
            return Task.CompletedTask;
        }

        public async Task Receive(Message message)
        {
            if (Channels.ContainsKey(message.ChannelName))
            {
                await Channels[message.ChannelName].Receive(message);
            }
        }
    }
}
