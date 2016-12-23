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
        private IDictionary<string, Channel> _channels = new ConcurrentDictionary<string, Channel>();

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
            if (!_channels.ContainsKey(name))
            {
                var channel = MessageHub.Channel.Create().WithName(name);
                channel.MessageSending += Channel_MessageSending;

                _channels[name] = channel;
            }

            return _channels[name];
        }

        private async void Channel_MessageSending(object sender, MessageEventArgs e)
        {
            Channel sendingChannel = sender as Channel;

            if (sendingChannel == null || !_channels.Values.Any(c => c.Id == sendingChannel.Id))
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
            if (_channels.ContainsKey(message.ChannelName))
            {
                await _channels[message.ChannelName].Receive(message);
            }
        }
    }
}
