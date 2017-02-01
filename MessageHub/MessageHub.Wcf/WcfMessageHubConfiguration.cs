namespace MessageHub.Wcf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;
    using MessageHub.Interfaces;

    public class WcfMessageHubConfiguration : IHubConfiguration
    {
        public string RemoteEndpoint { get; set; }

        public Binding Binding { get; set; }
    }
}
