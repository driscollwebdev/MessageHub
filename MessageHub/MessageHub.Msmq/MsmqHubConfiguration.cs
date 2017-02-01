namespace MessageHub.Msmq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MessageHub.Interfaces;

    public class MsmqHubConfiguration : IHubConfiguration
    {
        public string LocalQueueName { get; set; }

        public string RemoteQueuePath { get; set; }
    }
}
