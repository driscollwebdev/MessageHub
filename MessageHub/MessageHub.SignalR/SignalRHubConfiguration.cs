namespace MessageHub.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MessageHub.Interfaces;

    public sealed class SignalRHubConfiguration : IHubConfiguration
    {
        public string RemoteEndpoint { get; set; }

        public string HubName { get; set; }
    }
}
