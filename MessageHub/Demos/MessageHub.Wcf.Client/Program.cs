using MessageHub.Hubs;
using MessageHub.Interfaces;
using MessageHub.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessageHub.Wcf.Client
{
    class Program
    {
        const string CMD_QUIT = "/quit";
        const string CMD_EXIT = "/exit";

        static void Main(string[] args)
        {
            RemoteMessageHub msgHub = WcfMessageHub.Create()
                                                .WithRemoteEndpoint("net.tcp://localhost:9099/DemoHub")
                                                .WithBinding(new NetTcpBinding());

            msgHub.Connect().Wait();

            Guid userGuid = msgHub.Channel("Default").AddReceiver("public", (msg) =>
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

            ChatMessage joined = new ChatMessage
            {
                Username = "admin",
                MessageText = $"{username} has joined."
            };

            msgHub.Channel("Default").Send("public", joined).Wait();

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

            msgHub.Channel("Default").RemoveReceiver("public", userGuid);

            ChatMessage left = new ChatMessage
            {
                Username = "admin",
                MessageText = $"{username} has left."
            };

            msgHub.Channel("Default").Send("public", left).Wait();

            msgHub.Disconnect();
        }
    }
}
