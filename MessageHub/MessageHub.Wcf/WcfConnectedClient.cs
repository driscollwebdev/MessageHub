namespace MessageHub.Wcf
{
    using Interfaces;
    using System;

    public class WcfConnectedClient : IConnectedClient
    {
        public Guid Id { get; set; }

        public IMessageHubServiceReceiver ClientCallback { get; set; }
    }
}
