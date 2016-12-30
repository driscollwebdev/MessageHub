namespace MessageHub
{
    using System;
    using Interfaces;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Namespace = "")]
    public abstract class MessageHubServiceReceiver : IMessageHubServiceReceiver
    {
        private Guid _id = Guid.NewGuid();

        [DataMember]
        public Guid Id
        {
            [OperationContract]
            get
            {
                return _id;
            }
        }

        [OperationContract]
        public abstract void Receive(Guid hubId, Message message);
    }
}
