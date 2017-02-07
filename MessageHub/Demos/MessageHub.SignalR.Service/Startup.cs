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
        private static DemoHub _demoHub = new DemoHub(new AppConnectedClientRepository<HubConnectedClient>());

        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.Register(typeof(DemoHub), () => _demoHub);

            app.Map("/messagehub", map => {
                map.UseCors(CorsOptions.AllowAll);

                map.RunSignalR();
            });
        }
    }
}
