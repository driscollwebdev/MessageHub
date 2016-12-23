using System;
using System.Threading.Tasks;

namespace MessageHub.Interfaces
{
    public interface IMessageHub
    {
        Guid Id { get; }

        Channel Channel(string name);

        Task Broadcast(Message message);

        Task Receive(Message message);
    }
}
