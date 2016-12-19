namespace MessageHub.Lib
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A class representing a specific channel/topic of interest
    /// </summary>
    public class Channel
    {
        private IDictionary<string, IList<Tuple<Guid, Action<object>>>> _receivers = new Dictionary<string, IList<Tuple<Guid, Action<object>>>>();

        public string Name { get; set; }

        public IMessageHub Hub { get; set; }

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

        public Channel WithHub(IMessageHub hub)
        {
            Hub = hub;
            return this;
        }

        public void Send<TData>(string messageType, TData data)
        {
            Message message = Message.Create()
                                     .WithChannelName(Name)
                                     .WithType(messageType)
                                     .WithData(data);

            Receive(message);

            if (Hub != null)
            {
                message.HubId = Hub.Id;
                Hub.Broadcast(message);
            }
        }

        public Guid AddReceiver(string messageType, Action<object> action)
        {
            Guid receiverGuid = Guid.NewGuid();
            AddReceiver(messageType, action, receiverGuid);

            return receiverGuid;
        }

        public void AddReceiver(string messageType, Action<object> action, Guid receiverGuid)
        {
            IList<Tuple<Guid, Action<object>>> recList = new List<Tuple<Guid, Action<object>>>();
            if (_receivers.ContainsKey(messageType))
            {
                recList = _receivers[messageType];
            }

            recList.Add(new Tuple<Guid, Action<object>>(receiverGuid, action));

            _receivers[messageType] = recList;
        }

        public bool RemoveReceiver(string messageType, Guid receiverGuid)
        {
            throw new NotImplementedException();
        }

        internal void Receive(Message message)
        {
            if (message.HubId != null && Hub != null && message.HubId == Hub.Id)
            {
                return;
            }

            if (message.ChannelName != Name)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Type))
            {
                return;
            }

            var receiverActions = GetReceiverActions(message.Type);

            foreach(var action in receiverActions.AsParallel())
            {
                action(message.GetDataObject());
            }
        }

        private IList<Guid> GetReceiverIds(string messageType)
        {
            IList<Guid> instances = new List<Guid>();

            if (_receivers.ContainsKey(messageType))
            {
                instances = _receivers[messageType].Select(r => r.Item1).ToList();
            }

            return instances;
        }

        private IList<Action<object>> GetReceiverActions(string messageType)
        {
            IList<Action<object>> actions = new List<Action<object>>();

            if (_receivers.ContainsKey(messageType))
            {
                actions = _receivers[messageType].Select(r => r.Item2).ToList();
            }

            return actions;
        }
    }
}
