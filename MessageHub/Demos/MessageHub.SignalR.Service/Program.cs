namespace MessageHub.SignalR.Service
{
    using System;
    using Microsoft.Owin.Hosting;

    public class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://*:8088"))
            {
                Console.WriteLine("Server running at http://*:8088");
                Console.ReadLine();
            }
        }
    }
}
