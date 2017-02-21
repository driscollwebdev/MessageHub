namespace MessageHub.SignalR
{
    using Interfaces;

    public sealed class SignalRHubConfiguration : IHubConfiguration
    {
        public bool UseEncryption { get; set; }

        public SerializationType DefaultSerializationType { get; set; } = SerializationType.None;

        public string RemoteEndpoint { get; set; }

        public string HubName { get; set; }
    }
}
