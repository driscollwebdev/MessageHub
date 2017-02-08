namespace MessageHub
{
    using System.Runtime.Serialization;

    /// <summary>
    /// A class that represents an encrypted message
    /// </summary>
    [DataContract(Namespace = "")]
    public class SecureMessageContainer
    {
        /// <summary>
        /// Gets or sets a base64-encoded value for the asymmetrically-encrypted symmetric key used to encrypt the data
        /// </summary>
        [DataMember]
        public string EncryptedKey { get; set; }

        /// <summary>
        /// Gets or sets a base64-encoded value for the asymmetrically-encrypted init vector used to encrypt the data
        /// </summary>
        [DataMember]
        public string EncryptedIV { get; set; }

        /// <summary>
        /// Gets or sets a base64-encoded value for the symetrically encrypted message data
        /// </summary>
        [DataMember]
        public string EncryptedData { get; set; }
    }
}
