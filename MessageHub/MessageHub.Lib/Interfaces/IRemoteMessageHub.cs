namespace MessageHub.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IRemoteMessageHub : IMessageHub
    {
        Task Connect();

        void Disconnect();

        IRemoteMessageHub WithConfiguration(IHubConfiguration config);
    }
}
