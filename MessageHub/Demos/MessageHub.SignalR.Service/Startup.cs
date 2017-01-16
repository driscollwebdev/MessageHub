using MessageHub.Repositories;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(MessageHub.SignalR.Service.Startup))]
namespace MessageHub.SignalR.Service
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.Register(typeof(DemoHub),
                () => new DemoHub(new AppConnectedClientRepository<HubConnectedClient>()));

            app.Map("/messagehub", map => {
                map.UseCors(CorsOptions.AllowAll);

                map.RunSignalR();
            });
        }
    }
}
