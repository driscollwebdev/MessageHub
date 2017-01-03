namespace MessageHub.Wcf.Service
{
    using Interfaces;
    using Wcf;
    using System;
    using System.ServiceModel;

    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost svc = new ServiceHost(typeof(WcfMessageHubService));
            svc.AddServiceEndpoint(typeof(IMessageHubService), new NetTcpBinding(), "net.tcp://localhost:9099/DemoHub");
            svc.Open();
            Console.WriteLine("Server running at net.tcp://localhost:9099/DemoHub");

            Console.ReadLine();
            svc.Close();
        }
    }
}
