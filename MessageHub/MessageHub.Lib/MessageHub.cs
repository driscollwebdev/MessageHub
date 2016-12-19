namespace MessageHub.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MessageHub
    {
        private IDictionary<string, Channel> _channels = new Dictionary<string, Channel>();

        private readonly Guid _hubId = Guid.NewGuid();

        private MessageHub() { }

        public static MessageHub Create()
        {
            return new MessageHub();
        }

        public Channel Channel(string name)
        {
            if (!_channels.ContainsKey(name))
            {
                _channels[name] = Lib.Channel.Create().WithName(name).WithHubId(_hubId);
            }

            return _channels[name];
        }
    }
}
