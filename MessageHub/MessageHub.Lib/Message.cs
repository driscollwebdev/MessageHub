namespace MessageHub
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    [DataContract(Namespace = "")]
    public sealed class Message
    {
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ChannelName { get; set; }

        [DataMember]
        public Guid ChannelId { get; set; }

        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public string DataType { get; set; }

        public Message() { }

        public static Message Create()
        {
            return new Message();
        }

        public Message WithType(string type)
        {
            Type = type;
            return this;
        }

        public Message ToChannelName(string channelName)
        {
            ChannelName = channelName;
            return this;
        }

        public Message FromChannelId(Guid channelId)
        {
            ChannelId = channelId;
            return this;
        }

        public Message WithData(object data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, data);
                byte[] serializedData = ms.ToArray();
                Data = System.Convert.ToBase64String(serializedData);
                DataType = data.GetType().FullName;
            }

            return this;
        }

        public object GetDataObject()
        {
            object dataObject = null;

            byte[] decodedData = System.Convert.FromBase64String(Data);
            using (MemoryStream ms = new MemoryStream(decodedData))
            {
                BinaryFormatter bf = new BinaryFormatter();
                dataObject = bf.Deserialize(ms);
            }

            return dataObject;
        }
    }
}
