using FluentTerminal.SystemTray.Services;
using Owin;
using System.Web.Http;
using Unity;

namespace FluentTerminal.SystemTray
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            var container = new UnityContainer();
            container.RegisterType<TerminalsManager>();

            config.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            //appBuilder.UseFileServer()
            appBuilder.UseStaticFiles("/Client");
            appBuilder.UseWebApi(config);
        }
    }
}
