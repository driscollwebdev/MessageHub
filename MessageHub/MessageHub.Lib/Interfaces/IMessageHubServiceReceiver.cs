using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MessageHub.Interfaces
{
    public interface IMessageHubServiceReceiver
    {
        [DataMember]
        Guid Id { get; }
    }
}
