namespace MessageHub.Msmq
{
    using Interfaces;

    public class MsmqHubConfiguration : IHubConfiguration
    {
        public bool UseEncryption { get; set; }

        public SerializationType DefaultSerializationType { get; set; } = SerializationType.None;

        public string LocalQueueName { get; set; }

        public string RemoteQueuePath { get; set; }
    }
}
