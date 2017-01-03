namespace MessageHub.Interfaces
{
    using System;
    using System.ServiceModel;

    [ServiceContract(Namespace = "", CallbackContract = typeof(IMessageHubServiceReceiver), SessionMode = SessionMode.Required)]
    public interface IMessageHubService
    {
        [OperationContract]
        void Send(Guid senderId, Message message);

        [OperationContract]
        void AddReceiver(Guid receiverId);

        [OperationContract]
        void RemoveReceiver(Guid receiverId);

    }
}
