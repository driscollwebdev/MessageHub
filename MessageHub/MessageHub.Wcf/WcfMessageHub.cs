namespace MessageHub.Wcf
{
    using System;
    using System.Threading.Tasks;
    using Hubs;
    using Interfaces;

    public sealed class WcfMessageHub : RemoteMessageHub, IMessageHub, IMessageHubServiceReceiver
    {
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

        public static WcfMessageHub Create()
        {
            return new WcfMessageHub();
        }

        public static WcfMessageHub Create(LocalMessageHub inner)
        {
            return new WcfMessageHub(inner);
        }

        public WcfMessageHub WithRemote(IMessageHubService remote)
        {
            _proxy = remote;
            _proxy.AddReceiver(Id);

            return this;
        }

        public override void Disconnect()
        {
            _proxy.RemoveReceiver(Id);
        }
    }
}
