using MessageHub.Msmq;
using MessageHub.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHub.Msmq.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MsmqMessageHubService service = new MsmqMessageHubService(".\\private$\\AppHub", new AppConnectedClientRepository<MsmqConnectedClient>()))
            {
                Console.ReadKey();
            }
        }
    }
}
