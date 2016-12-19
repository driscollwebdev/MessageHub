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

        public Guid Id { get; } = Guid.NewGuid();

        private MessageHub() { }

        public static MessageHub Create()
        {
            return new MessageHub();
        }

        public Channel Channel(string name)
        {
            if (!_channels.ContainsKey(name))
            {
                _channels[name] = Lib.Channel.Create().WithName(name).WithHub(this);
            }

            return _channels[name];
        }

        async Task IMessageHub.Broadcast(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
