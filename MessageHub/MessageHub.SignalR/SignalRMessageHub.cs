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

        public static IMessageHub Create()
        {
            return new SignalRMessageHub();
        }

        public static IMessageHub Create(LocalMessageHub inner)
        {
            return new SignalRMessageHub(inner);
        }

        public IMessageHub WithRemote(IHubProxy remote)
        {
            _proxy = remote;
            _proxy.On<Guid, Message>("Receive", (fromHubId, message) => Receive(fromHubId, message));

            return this;
        }
    }
}
