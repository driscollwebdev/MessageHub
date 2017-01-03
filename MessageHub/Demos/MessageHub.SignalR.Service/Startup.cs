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
            app.Map("/messagehub", map => {
                map.UseCors(CorsOptions.AllowAll);

                map.RunSignalR();
            });
        }
    }
}
