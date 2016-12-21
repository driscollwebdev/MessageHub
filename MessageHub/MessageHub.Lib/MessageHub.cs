namespace MessageHub.Lib
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MessageHub : IMessageHub
    {
        private IDictionary<string, Channel> _channels = new Dictionary<string, Channel>();

        public event EventHandler<MessageEventArgs> Broadcasting;

        public Guid Id { get; } = Guid.NewGuid();

        private MessageHub() { }

        public static IMessageHub Create()
        {
            return new MessageHub();
        }

        public Channel Channel(string name)
        {
            if (!_channels.ContainsKey(name))
            {
                var channel = Lib.Channel.Create().WithName(name);
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

        public virtual Task Broadcast(Message message)
        {
            Broadcasting(this, new MessageEventArgs(message));
            return Task.CompletedTask;
        }

        public virtual async Task Receive(Message message)
        {
            if (_channels.ContainsKey(message.ChannelName))
            {
                await _channels[message.ChannelName].Receive(message);
            }
        }
    }
}
