namespace MessageHub.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IRemoteMessageHub : IMessageHub
    {
        Task Connect();

        void Disconnect();

        IRemoteMessageHub WithConfiguration(IHubConfiguration config);

        void Configure<THubConfiguration>(Action<THubConfiguration> configure) where THubConfiguration : IHubConfiguration;
    }
}
