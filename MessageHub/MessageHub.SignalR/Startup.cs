using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MessageHub.SignalR.Startup))]
namespace MessageHub.SignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR("/messagehub", new Microsoft.AspNet.SignalR.HubConfiguration());
        }
    }
}
