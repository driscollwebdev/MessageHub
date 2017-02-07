namespace MessageHub.Interfaces
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    /// <summary>
    /// An interface for message receivers
    /// </summary>
    public interface IMessageHubServiceReceiver
    {
        /// <summary>
        /// Gets a value for the Id of the receiver
        /// </summary>
        [DataMember]
        Guid Id { [OperationContract]get; }

        /// <summary>
        /// Receives a message from the service
        /// </summary>
        /// <param name="senderId">The Id of the sender</param>
        /// <param name="message">The message received</param>
        [OperationContract]
        void Receive(Guid senderId, Message message);

        [OperationContract(Name = "ReceiveSecure")]
        void Receive(Guid senderId, SecureMessageContainer secureMessage);
    }
}
