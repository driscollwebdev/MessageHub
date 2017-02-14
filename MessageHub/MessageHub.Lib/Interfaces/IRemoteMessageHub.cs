namespace MessageHub.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IRemoteMessageHub : IMessageHub
    {
        /// <summary>
        /// Connects the hub instance with a remote hub service.
        /// </summary>
        /// <returns>A task for continuation</returns>
        Task Connect();

        /// <summary>
        /// Disconnects the hub instance from the remote hub service.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sets the configuration for the hub-service connection
        /// </summary>
        /// <param name="config">The configuration object</param>
        /// <returns>The current instance / implementation of <see cref="IRemoteMessageHub"/></returns>
        IRemoteMessageHub WithConfiguration(IHubConfiguration config);

        /// <summary>
        /// Configures the local hub for hub-service connection using the passed delegate
        /// </summary>
        /// <typeparam name="THubConfiguration">The concrete implementation type of <see cref="IHubConfiguration"/></typeparam>
        /// <param name="configure">The delegate used to configure the hub</param>
        void Configure<THubConfiguration>(Action<THubConfiguration> configure) where THubConfiguration : IHubConfiguration;
    }
}
