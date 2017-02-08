namespace MessageHub
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// A data transfer object used to transfer relevant data about a client to a remote hub service
    /// </summary>
    [DataContract(Namespace = "")]
    public sealed class ConnectedClientData
    {
        /// <summary>
        /// Gets or sets a value for the client Id
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value for the client's public key
        /// </summary>
        [DataMember]
        public string PublicKey { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="ConnectedClientData"/> class
        /// </summary>
        public ConnectedClientData() { }

        /// <summary>
        /// Initializes an instance of the <see cref="ConnectedClientData"/> class
        /// </summary>
        /// <param name="id">The client Id</param>
        /// <param name="publicKey">The client public key</param>
        public ConnectedClientData(Guid id, string publicKey)
        {
            Id = id;
            PublicKey = publicKey;
        }
    }
}
