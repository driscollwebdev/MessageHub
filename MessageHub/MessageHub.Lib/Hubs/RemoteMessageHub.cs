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

        protected RemoteMessageHub(LocalMessageHub inner)
        {
            if (inner == null)
            {
                inner = (LocalMessageHub)LocalMessageHub.Create();
            }

            _innerHub = inner;
        }

        protected RemoteMessageHub() : this(null) { }
    }
}
