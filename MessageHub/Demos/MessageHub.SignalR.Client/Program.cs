using MessageHub.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace MessageHub.SignalR.Client
{
    class Program
    {
        const string CMD_QUIT = "/quit";
        const string CMD_EXIT = "/exit";

        static void Main(string[] args)
        {
            HubConnection connection = new HubConnection("http://localhost:8088/messagehub/");
            IHubProxy demoHubProxy = connection.CreateHubProxy("DemoHub");
            connection.Start().Wait();

            IMessageHub msgHub = SignalRMessageHub.Create().WithRemote(demoHubProxy);

            msgHub.Channel("Default").AddReceiver("public", (msg) =>
            {
                ChatMessage message = msg as ChatMessage;
                if (message != null)
                {
                    message.Received = DateTime.Now;
                    Console.WriteLine(message.ToString());
                }
                return Task.CompletedTask;
            });

            string username = "Guest";
            Console.Write("Enter your username: ");
            username = Console.ReadLine();

            Console.WriteLine($"Welcome, {username}");

            string outgoingMessage = Console.ReadLine();
            while (outgoingMessage != CMD_EXIT && outgoingMessage != CMD_QUIT)
            {
                ChatMessage chat = new ChatMessage
                {
                    Username = username,
                    MessageText = outgoingMessage
                };

                msgHub.Channel("Default").Send("public", chat).Wait();

                outgoingMessage = Console.ReadLine();
            }

            ((SignalRMessageHub)msgHub).Disconnect();
            connection.Stop();
        }
    }
}
