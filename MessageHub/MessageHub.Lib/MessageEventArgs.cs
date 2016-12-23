namespace MessageHub
{
    using System;

    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        public MessageEventArgs(Message message) : base()
        {
            Message = message;
        }
    }
}