namespace MessageHub.Interfaces
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an interface that must be implemented by all message hub classes
    /// </summary>
    public interface IMessageHub
    {
        /// <summary>
        /// Gets the Id of the current hub
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets or creates and gets the channel identified by name.
        /// </summary>
        /// <param name="name">The name of the channel</param>
        /// <returns>An instance of <see cref="Channel"/></returns>
        Channel Channel(string name);

        /// <summary>
        /// Broadcasts an outgoing message to subscribers
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <returns>A task for continuation purposes</returns>
        Task Broadcast(Message message);

        /// <summary>
        /// Handles an incoming message to the hub
        /// </summary>
        /// <param name="message">The message that is received</param>
        /// <returns>A task for continuation purposes</returns>
        Task Receive(Message message);
    }
}
