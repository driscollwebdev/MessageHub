namespace MessageHub.Msmq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    [XmlInclude(typeof(MsmqConnectedClient))]
    [XmlInclude(typeof(MessageHub.Message))]
    [XmlInclude(typeof(Guid))]
    [Serializable]
    public sealed class MessageEnvelope
    {
        public Guid SenderId { get; set; }

        public HubServiceOperation ServiceOp { get; set; }

        public object Contents { get; set; }
    }
}
