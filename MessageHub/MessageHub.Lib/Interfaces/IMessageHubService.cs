namespace MessageHub.Interfaces
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// An interface representing a remote message hub service
    /// </summary>
    [ServiceContract(Namespace = "", CallbackContract = typeof(IMessageHubServiceReceiver), SessionMode = SessionMode.Required)]
    public interface IMessageHubService
    {
        /// <summary>
        /// Sends a message to all receivers, except for the sender identified by senderId
        /// </summary>
        /// <param name="senderId">The Id of the sending hub</param>
        /// <param name="message">The message to send</param>
        [OperationContract]
        void Send(Guid senderId, Message message);

        [OperationContract]
        void SendSecure(Guid senderId, SecureMessageContainer secureMessage);

        /// <summary>
        /// Adds a receiver (usually a hub) to listen for messages from this service
        /// </summary>
        /// <param name="receiverId">The Id of the receiver</param>
        [OperationContract]
        void AddReceiver(ConnectedClientData clientData);

        /// <summary>
        /// Removes a receiver from listening for messages from this service
        /// </summary>
        /// <param name="receiverId">The Id of the receiver</param>
        [OperationContract]
        void RemoveReceiver(Guid receiverId);

        /// <summary>
        /// Gets a value for the service's public key
        /// </summary>
        /// <returns>A string containing the public key of the service</returns>
        [OperationContract]
        string GetServiceKey();

    }
}
