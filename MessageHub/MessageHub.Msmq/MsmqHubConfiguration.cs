namespace MessageHub.Msmq
{
    using Interfaces;

    public class MsmqHubConfiguration : IHubConfiguration
    {
        public bool UseEncryption { get; set; }

        public string LocalQueueName { get; set; }

        public string RemoteQueuePath { get; set; }
    }
}
