namespace MessageHub.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A class representing a specific channel/topic of interest
    /// </summary>
    public class Channel
    {
        public string Name { get; set; }

        public Guid? HubId { get; set; }

        private Channel() { }

        public static Channel Create()
        {
            return new Channel();
        }

        public Channel WithName(string name)
        {
            Name = name;
            return this;
        }

        public Channel WithHubId(Guid hubId)
        {
            HubId = hubId;
            return this;
        }
    }
}
