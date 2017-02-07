namespace MessageHub.Interfaces
{
    using System;

    /// <summary>
    /// An interface that represents a client connected to a remote hub service
    /// </summary>
    public interface IConnectedClient
    {
        /// <summary>
        /// Gets or sets a value for the client Id.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value for the client's public key
        /// </summary>
        string PublicKey { get; set; }
    }
}
