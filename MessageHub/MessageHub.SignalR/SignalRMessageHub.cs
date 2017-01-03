namespace MessageHub.SignalR
{
    using System;
    using System.Threading.Tasks;
    using Hubs;
    using Interfaces;
    using Microsoft.AspNet.SignalR.Client;

    public sealed class SignalRMessageHub : RemoteMessageHub, IMessageHub, IMessageHubServiceReceiver
    {
        private IHubProxy _proxy;

        private SignalRMessageHub() : base() { }

        private SignalRMessageHub(LocalMessageHub inner) : base(inner) { }

        public override Task Broadcast(Message message)
        {
            return _proxy.Invoke("Send", Id, message);
        }

        public void Receive(Guid hubId, Message message)
        {
            base.Receive(message);
        }

        public static SignalRMessageHub Create()
        {
            return new SignalRMessageHub();
        }

        public static SignalRMessageHub Create(LocalMessageHub inner)
        {
            return new SignalRMessageHub(inner);
        }

        public SignalRMessageHub WithRemote(IHubProxy remote)
        {
            _proxy = remote;
            _proxy.On<Guid, Message>("Receive", (fromHubId, message) => Receive(fromHubId, message));

            _proxy.Invoke<Guid>("AddReceiver", Id);

            return this;
        }

        public override void Disconnect()
        {
            _proxy.Invoke<Guid>("RemoveReceiver", this.Id);
        }
    }
}
