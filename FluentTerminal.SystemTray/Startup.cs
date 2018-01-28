using FluentTerminal.SystemTray.Services;
using Owin;
using System.Web.Http;
using Unity;
using Unity.Lifetime;

namespace FluentTerminal.SystemTray
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            var container = new UnityContainer();
            container.RegisterType<TerminalsManager>(new ContainerControlledLifetimeManager());

            config.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            appBuilder.UseStaticFiles("/Client");
            appBuilder.UseWebApi(config);
        }
    }
}
