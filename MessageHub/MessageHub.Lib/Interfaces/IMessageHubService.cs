namespace MessageHub.Interfaces
{
    using System;
    using System.ServiceModel;

    [ServiceContract(Namespace = "", CallbackContract = typeof(IMessageHubServiceReceiver), SessionMode = SessionMode.Required)]
    public interface IMessageHubService
    {
        [OperationContract]
        void Send(Guid fromHubId, Message message);

        [OperationContract]
        void AddReceiver(IMessageHubServiceReceiver receiver);

        [OperationContract]
        void RemoveReceiver(Guid receiverId);

    }
}
