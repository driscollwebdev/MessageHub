namespace MessageHub.Lib
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "")]
    public abstract class Message
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ChannelName { get; set; }

        [DataMember]
        public Guid? HubId { get; set; }

        [DataMember]
        public virtual string Data { get; set; }

        protected Message() { }

        public static TMessage Create<TMessage>() where TMessage : Message, new()
        {
            return Activator.CreateInstance<TMessage>();
        }

        public TMessage WithType<TMessage>(string type) where TMessage : Message
        {
            Type = type;
            return this as TMessage;
        }

        public TMessage WithChannelName<TMessage>(string channelName) where TMessage : Message
        {
            ChannelName = channelName;
            return this as TMessage;
        }

        public TMessage WithHubId<TMessage>(Guid hubId) where TMessage : Message
        {
            HubId = hubId;
            return this as TMessage;
        }

        public TMessage WithData<TMessage>(string data) where TMessage : Message
        {
            Data = data;
            return this as TMessage;
        }
    }
}
