namespace MessageHub.Wcf
{
    using System;
    using System.Threading.Tasks;
    using Hubs;
    using Interfaces;
    using System.ServiceModel;

    public sealed class WcfMessageHub : RemoteMessageHub, IMessageHub, IMessageHubServiceReceiver
    {
        private string _remoteUri;
        private DuplexChannelFactory<IMessageHubService> _channelFactory;
        private System.ServiceModel.Channels.Binding _channelBinding;
        private IMessageHubService _proxy;

        private WcfMessageHub() : base() { }

        private WcfMessageHub(LocalMessageHub inner) : base(inner) { }

        public override Task Broadcast(Message message)
        {
            _proxy.Send(Id, message);
            return Task.CompletedTask;
        }

        public void Receive(Guid hubId, Message message)
        {
            base.Receive(message);
        }

        public static IRemoteMessageHub Create()
        {
            return new WcfMessageHub();
        }

        public static IRemoteMessageHub Create(LocalMessageHub inner)
        {
            return new WcfMessageHub(inner);
        }

        public override IRemoteMessageHub WithConfiguration(IHubConfiguration config)
        {
            WcfMessageHubConfiguration wcfConfig = (WcfMessageHubConfiguration)config;

            _remoteUri = wcfConfig.RemoteEndpoint;
            _channelBinding = wcfConfig.Binding;

            return this;
        }

        public override Task Connect()
        {
            if (_channelFactory != null && _channelFactory.State == CommunicationState.Opened)
            {
                return Task.CompletedTask;
            }

            _channelFactory = new DuplexChannelFactory<IMessageHubService>(this, _channelBinding, new EndpointAddress(_remoteUri));
            _proxy = _channelFactory.CreateChannel();
            _proxy.AddReceiver(Id);

            return Task.CompletedTask;
        }

        public override void Disconnect()
        {
            _proxy.RemoveReceiver(Id);
            _channelFactory.Close();
        }
    }
}
