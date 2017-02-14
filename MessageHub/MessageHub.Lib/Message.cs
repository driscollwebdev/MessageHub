namespace MessageHub
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// A class representing a message that can be published to a hub via a channel
    /// </summary>
    [DataContract(Namespace = "")]
    public sealed class Message
    {
        /// <summary>
        /// Gets or sets a value for the Id of this message
        /// </summary>
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets a value for the type of this message
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value for the channel name associated with this message
        /// </summary>
        [DataMember]
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets a value for the Id of the specific channel that is sending the message
        /// </summary>
        [DataMember]
        public Guid ChannelId { get; set; }

        /// <summary>
        /// Gets or sets a value for the data payload passed with this message
        /// </summary>
        [DataMember]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets a value for the object type of the data being sent
        /// </summary>
        [DataMember]
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets a value for the serialization type of the data payload
        /// </summary>
        [DataMember]
        public SerializationType SerializationType { get; set; } = SerializationType.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class
        /// </summary>
        public Message() { }

        /// <summary>
        /// Creates a new instance of the <see cref="Message"/> class
        /// </summary>
        /// <returns>An instance of the <see cref="Message"/> class</returns>
        public static Message Create()
        {
            return new Message();
        }

        /// <summary>
        /// Sets the type of the message
        /// </summary>
        /// <param name="type">The message type</param>
        /// <returns>The current instance</returns>
        public Message WithType(string type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Sets the channel of the current message
        /// </summary>
        /// <param name="channelName">The channel name associated with this message</param>
        /// <returns>The current instance</returns>
        public Message ToChannelName(string channelName)
        {
            ChannelName = channelName;
            return this;
        }

        /// <summary>
        /// Sets the sending channel Id for the current message
        /// </summary>
        /// <param name="channelId">The sending channel Id</param>
        /// <returns>The current instance</returns>
        public Message FromChannelId(Guid channelId)
        {
            ChannelId = channelId;
            return this;
        }

        /// <summary>
        /// Sets the data payload associated with the current instance
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The current instance</returns>
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

        /// <summary>
        /// Sets the data payload associated with the current instance using the serialization type provided
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="serializationType">The serialization type</param>
        /// <returns>The current instance</returns>
        public Message WithData(object data, SerializationType serializationType)
        {
            SerializationType = serializationType;

            return WithData(data);
        }

        /// <summary>
        /// Gets the data payload for this message as an object
        /// </summary>
        /// <returns>The message data</returns>
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

        /// <summary>
        /// Gets the data payload for this message as an object using the specified type information
        /// </summary>
        /// <param name="type">The type of the data payload</param>
        /// <returns>The message data</returns>
        public object GetDataObject(Type type)
        {
            object dataObject = GetSerializedDataObject();

            return Deserialize(dataObject, type);
        }

        /// <summary>
        /// Gets the data payload for this message as an object of the specified type
        /// </summary>
        /// <typeparam name="TData">The expected type of the data</typeparam>
        /// <returns>An instance of the specified type</returns>
        public TData GetData<TData>()
        {
            object dataObject = GetSerializedDataObject();

            return Deserialize<TData>(dataObject);
        }

        /// <summary>
        /// Unpacks the Data value to a serialized object
        /// </summary>
        /// <returns>An object representing serialized data</returns>
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

        /// <summary>
        /// Serializes the passed data with the SerializationType set for this instance
        /// </summary>
        /// <param name="data">The data to serialize</param>
        /// <returns>An object representing serialized data</returns>
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
                case SerializationType.None:
                default:
                    serializedData = data;
                    break;
            }

            return serializedData;
        }

        /// <summary>
        /// Deserializes the passed data with the SerializationType set for this instance
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <returns>An object representing deserialized data</returns>
        /// <exception cref="NotSupportedException">Thrown if XML serialization is specified, but there is no type information provided</exception>
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
                case SerializationType.None:
                default:
                    deserializedData = data;
                    break;
            }

            return deserializedData;
        }

        /// <summary>
        /// Deserializes the passed data to the specified type using the SerializationType set for this instance
        /// </summary>
        /// <param name="data">The data to deserialize</param>
        /// <param name="type">The expected type of the data</param>
        /// <returns>An object representing deserialized data</returns>
        /// <exception cref="ArgumentNullException">Thrown if the type parameter is null</exception>
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
                case SerializationType.None:
                default:
                    deserializedData = data;
                    break;
            }

            return deserializedData;
        }

        /// <summary>
        /// Deserializes the passed data to the specified type
        /// </summary>
        /// <typeparam name="TData">The expected type of the data</typeparam>
        /// <param name="data">The data to deserialize</param>
        /// <returns>An object representing deserialized data</returns>
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
                case SerializationType.None:
                default:
                    deserializedData = (TData)data;
                    break;
            }

            return deserializedData;
        }
    }
}
