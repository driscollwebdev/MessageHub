namespace MessageHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    [DataContract(Namespace = "")]
    public class SecureMessageContainer
    {
        [DataMember]
        public byte[] EncryptedKey { get; set; }

        [DataMember]
        public byte[] EncryptedIV { get; set; }

        [DataMember]
        public byte[] EncryptedData { get; set; }
    }
}
