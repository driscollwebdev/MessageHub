﻿namespace MessageHub.SignalR
{
    using System;
    using System.Threading.Tasks;
    using Hubs;
    using Interfaces;
    using Microsoft.AspNet.SignalR.Client;

    public sealed class SignalRMessageHub : RemoteMessageHub, IMessageHub, IMessageHubServiceReceiver
    {
        private IHubProxy _proxy;

        private HubConnection _remoteConnection;

        private string _remoteUri;

        private string _remoteHubName;

        private SignalRMessageHub() : base() { }

        private SignalRMessageHub(LocalMessageHub inner) : base(inner) { }

        public override Task Broadcast(Message message)
        {
            return _proxy.Invoke("Send", Id, message);
        }

        public override Task Broadcast(SecureMessageContainer secureMessage)
        {
            return _proxy.Invoke("SendSecure", Id, secureMessage);
        }

        public void Receive(Guid hubId, Message message)
        {
            base.Receive(message);
        }

        public void Receive(Guid hubId, SecureMessageContainer secureMessage)
        {
            base.Receive(secureMessage);
        }

        public static IRemoteMessageHub Create()
        {
            return new SignalRMessageHub();
        }

        public static IRemoteMessageHub Create(LocalMessageHub inner)
        {
            return new SignalRMessageHub(inner);
        }

        public override IRemoteMessageHub WithConfiguration(IHubConfiguration config)
        {
            SignalRHubConfiguration hubConfig = (SignalRHubConfiguration)config;

            _remoteUri = hubConfig.RemoteEndpoint;
            _remoteHubName = hubConfig.HubName;

            return base.WithConfiguration(config);
        }

        public override async Task Connect()
        {
            if (_remoteConnection != null && _remoteConnection.State == ConnectionState.Connected)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_remoteUri))
            {
                throw new InvalidOperationException("You must set the remote endpoint by calling WithRemoteEndpoint first.");
            }

            if (string.IsNullOrWhiteSpace(_remoteHubName))
            {
                throw new InvalidOperationException("You must set the remote hub name by calling WithHubName first.");
            }

            _remoteConnection = new HubConnection(_remoteUri);
            _proxy = _remoteConnection.CreateHubProxy(_remoteHubName);

            await _remoteConnection.Start();

            if (UseEncryption)
            {
                RemotePublicKey = await _proxy.Invoke<string>("GetServiceKey");
            }

            _proxy.On<Guid, Message>("Receive", (fromHubId, message) => Receive(fromHubId, message));
            _proxy.On<Guid, SecureMessageContainer>("ReceiveSecure", (senderId, container) => Receive(senderId, container));

            await _proxy.Invoke("AddReceiver", new ConnectedClientData(Id, PublicKey));
        }

        public override void Disconnect()
        {
            _proxy.Invoke("RemoveReceiver", this.Id);
            _remoteConnection.Stop();
        }
    }
}
