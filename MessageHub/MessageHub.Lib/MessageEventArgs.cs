namespace MessageHub
{
    using System;

    /// <summary>
    /// A class representing message event arguments
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The message
        /// </summary>
        public Message Message { get; private set; }

        /// <summary>
        /// Initializes an instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message</param>
        public MessageEventArgs(Message message) : base()
        {
            Message = message;
        }
    }
}