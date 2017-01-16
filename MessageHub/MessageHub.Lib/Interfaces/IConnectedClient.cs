namespace MessageHub.Interfaces
{
    using System;

    public interface IConnectedClient
    {
        Guid Id { get; set; }
    }
}
