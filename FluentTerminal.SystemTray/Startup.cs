using FluentTerminal.SystemTray.Services;
using GlobalHotKey;
using Owin;
using System.Web.Http;
using System.Windows.Threading;
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
            container.RegisterType<NotificationService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ToggleWindowService>(new ContainerControlledLifetimeManager());
            container.RegisterInstance(new HotKeyManager(), new ContainerControlledLifetimeManager());
            container.RegisterInstance(Dispatcher.CurrentDispatcher, new ContainerControlledLifetimeManager());

            config.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            appBuilder.UseWebApi(config);
        }
    }
}
