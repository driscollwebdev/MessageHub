using System;
using System.Runtime.Serialization;

namespace MessageHub
{
    [DataContract(Namespace = "")]
    public class ConnectedClientData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string PublicKey { get; set; }

        public ConnectedClientData() { }

        public ConnectedClientData(Guid id, string publicKey)
        {
            Id = id;
            PublicKey = publicKey;
        }
    }
}
