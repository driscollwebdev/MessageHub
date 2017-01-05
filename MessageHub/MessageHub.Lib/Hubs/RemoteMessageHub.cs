namespace MessageHub.Hubs
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;

    /// <summary>
    /// An abstract class representing a message hub that is connected to a remote server
    /// </summary>
    public abstract class RemoteMessageHub : IMessageHub
    {
        /// <summary>
        /// A contained local hub used for much of the basic operation of this hub
        /// </summary>
        private LocalMessageHub _innerHub;

        /// <summary>
        /// Gets a value for the Id of this instance
        /// </summary>
        public Guid Id
        {
            get
            {
                return _innerHub.Id;
            }
        }

        /// <summary>
        /// Broadcasts a message to all receivers. Must be overridden in a derived class.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <returns>A task for continuation</returns>
        public abstract Task Broadcast(Message message);

        /// <summary>
        /// Gets or creates the channel indicated by name
        /// </summary>
        /// <param name="name">The name of the channel to retrieve</param>
        /// <returns>An instance of the <see cref="Channel"/> class</returns>
        public Channel Channel(string name)
        {
            return _innerHub.Channel(name);
        }

        /// <summary>
        /// Receive a message from another source.
        /// </summary>
        /// <param name="message">The message to receive</param>
        /// <returns>A task for continuation purposes</returns>
        public virtual Task Receive(Message message)
        {
            return _innerHub.Receive(message);
        }

        /// <summary>
        /// Disconnects this hub from the remote service, performing any necessary cleanup tasks.
        /// </summary>
        public virtual void Disconnect()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMessageHub"/> class.
        /// </summary>
        /// <param name="inner">The inner hub</param>
        protected RemoteMessageHub(LocalMessageHub inner)
        {
            if (inner == null)
            {
                inner = (LocalMessageHub)LocalMessageHub.Create();
            }

            _innerHub = inner;
            _innerHub.Broadcasting += (s, e) => Broadcast(e.Message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMessageHub"/> class.
        /// </summary>
        protected RemoteMessageHub() : this(null) { }
    }
}
