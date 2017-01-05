namespace MessageHub
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using System.Xml.Serialization;

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

        [DataMember]
        public SerializationType SerializationType { get; set; } = SerializationType.Default;

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
                bf.Serialize(ms, Serialize(data));
                byte[] serializedData = ms.ToArray();
                Data = System.Convert.ToBase64String(serializedData);
                DataType = data.GetType().FullName;
            }

            return this;
        }

        public Message WithData(object data, SerializationType serializationType)
        {
            SerializationType = serializationType;

            return WithData(data);
        }

        public object GetDataObject()
        {
            object dataObject = GetSerializedDataObject();

            Type type = System.Type.GetType(DataType);
            if (type != null)
            {
                return Deserialize(dataObject, type);
            }

            return Deserialize(dataObject);
        }

        public object GetDataObject(Type type)
        {
            object dataObject = GetSerializedDataObject();

            return Deserialize(dataObject, type);
        }

        public TData GetData<TData>()
        {
            object dataObject = GetSerializedDataObject();

            return Deserialize<TData>(dataObject);
        }

        private object GetSerializedDataObject()
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

        private object Serialize(object data)
        {
            object serializedData = null;
            switch (SerializationType)
            {
                case SerializationType.Json:
                    serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    break;
                case SerializationType.Xml:
                    XmlSerializer serializer = new XmlSerializer(data.GetType());
                    using (StringWriter sw = new StringWriter())
                    {
                        using (XmlWriter xw = XmlWriter.Create(sw))
                        {
                            serializer.Serialize(xw, data);
                            serializedData = sw.ToString();
                        }
                    }
                    break;
                case SerializationType.Default:
                default:
                    serializedData = data;
                    break;
            }

            return serializedData;
        }

        private object Deserialize(object data)
        {
            if (data == null)
            {
                return null;
            }

            object deserializedData = null;

            switch (SerializationType)
            {
                case SerializationType.Json:
                    deserializedData = Newtonsoft.Json.JsonConvert.DeserializeObject((string)data);
                    break;
                case SerializationType.Xml:
                    throw new NotSupportedException("Cannot deserialize XML data without type information.");
                case SerializationType.Default:
                default:
                    deserializedData = data;
                    break;
            }

            return deserializedData;
        }

        private object Deserialize(object data, Type type)
        {
            if (data == null)
            {
                return null;
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            object deserializedData = null;

            switch(SerializationType)
            {
                case SerializationType.Json:
                    deserializedData = Newtonsoft.Json.JsonConvert.DeserializeObject((string)data, type);
                    break;
                case SerializationType.Xml:
                    XmlSerializer serializer = new XmlSerializer(type);
                    using (StringReader reader = new StringReader((string)data))
                    {
                        deserializedData = serializer.Deserialize(reader);
                    }
                    break;
                case SerializationType.Default:
                default:
                    deserializedData = data;
                    break;
            }

            return deserializedData;
        }

        private TData Deserialize<TData>(object data)
        {
            if (data == null)
            {
                return default(TData);
            }

            TData deserializedData = default(TData);

            switch(SerializationType)
            {
                case SerializationType.Json:
                    deserializedData = Newtonsoft.Json.JsonConvert.DeserializeObject<TData>((string)data);
                    break;
                case SerializationType.Xml:
                    XmlSerializer serializer = new XmlSerializer(typeof(TData));
                    using (StringReader reader = new StringReader((string)data))
                    {
                        deserializedData = (TData)serializer.Deserialize(reader);
                    }
                    break;
                case SerializationType.Default:
                default:
                    deserializedData = (TData)data;
                    break;
            }

            return deserializedData;
        }
    }
}
