namespace MessageHub.Hubs
{
    using System;
    using System.Threading.Tasks;
    using MessageHub.Interfaces;

    public abstract class RemoteMessageHub : IMessageHub
    {
        private LocalMessageHub _innerHub;

        public Guid Id
        {
            get
            {
                return _innerHub.Id;
            }
        }

        public abstract Task Broadcast(Message message);

        public Channel Channel(string name)
        {
            return _innerHub.Channel(name);
        }

        public virtual Task Receive(Message message)
        {
            return _innerHub.Receive(message);
        }

        public virtual void Disconnect()
        {
        }

        protected RemoteMessageHub(LocalMessageHub inner)
        {
            if (inner == null)
            {
                inner = (LocalMessageHub)LocalMessageHub.Create();
            }

            _innerHub = inner;
            _innerHub.Broadcasting += (s, e) => Broadcast(e.Message);
        }

        protected RemoteMessageHub() : this(null) { }
    }
}
