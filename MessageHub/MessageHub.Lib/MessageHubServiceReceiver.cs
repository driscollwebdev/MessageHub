namespace MessageHub
{
    using System;
    using Interfaces;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    /// <summary>
    /// Abstract implementation of the <see cref="IMessageHubServiceReceiver"/> interface
    /// </summary>
    [DataContract(Namespace = "")]
    public abstract class MessageHubServiceReceiver : IMessageHubServiceReceiver
    {
        /// <summary>
        /// Backing field for the Id property
        /// </summary>
        private Guid _id = Guid.NewGuid();

        /// <summary>
        /// The Id of this receiver
        /// </summary>
        [DataMember]
        public Guid Id
        {
            [OperationContract]
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Handles receiving a message
        /// </summary>
        /// <param name="senderId">The Id of the sender</param>
        /// <param name="message">The message to be sent</param>
        [OperationContract]
        public abstract void Receive(Guid senderId, Message message);

        /// <summary>
        /// Handles receiving an encrypted message
        /// </summary>
        /// <param name="senderId">The Id of the sender</param>
        /// <param name="secureMessage">The encrypted message to be sent</param>
        [OperationContract]
        public abstract void Receive(Guid senderId, SecureMessageContainer secureMessage);
    }
}
