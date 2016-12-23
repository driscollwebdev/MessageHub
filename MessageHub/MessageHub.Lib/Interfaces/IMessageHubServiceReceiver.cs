namespace MessageHub.Interfaces
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    public interface IMessageHubServiceReceiver
    {
        [DataMember]
        Guid Id { [OperationContract]get; }

        [OperationContract]
        void Receive(Guid hubId, Message message);
    }
}
