using Autofac;
using FluentTerminal.SystemTray.Services;
using GlobalHotKey;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace FluentTerminal.SystemTray
{
	public static class Program
	{
		private const string MutexName = "FluentTerminalMutex";

		[STAThread]
		public static void Main()
		{
			if (!Mutex.TryOpenExisting(MutexName, out Mutex mutex))
			{
				mutex = new Mutex(false, MutexName);

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				var containerBuilder = new ContainerBuilder();

				containerBuilder.RegisterType<UpdateService>().SingleInstance();
				containerBuilder.RegisterType<NotificationService>().SingleInstance();
				containerBuilder.RegisterType<TerminalsManager>().SingleInstance();
				containerBuilder.RegisterType<ToggleWindowService>().SingleInstance();
				containerBuilder.RegisterType<HotKeyManager>().SingleInstance();
				containerBuilder.RegisterType<SystemTrayApplicationContext>().SingleInstance();
				containerBuilder.RegisterType<AppCommunicationService>().SingleInstance();
				containerBuilder.RegisterInstance(Dispatcher.CurrentDispatcher).SingleInstance();

				var container = containerBuilder.Build();

				var appCommunicationService = container.Resolve<AppCommunicationService>();
				appCommunicationService.StartAppServiceConnection();
				Task.Run(() => CheckForNewVersion(container.Resolve<UpdateService>(), container.Resolve<NotificationService>()));

				Application.Run(container.Resolve<SystemTrayApplicationContext>());

				mutex.Close();
			}
			else
			{
				var eventWaitHandle = EventWaitHandle.OpenExisting(AppCommunicationService.EventWaitHandleName, System.Security.AccessControl.EventWaitHandleRights.Modify);
				eventWaitHandle.Set();
			}
		}

		private static void CheckForNewVersion(UpdateService updateService, NotificationService notificationService)
		{
			var newVersion = updateService.IsNewerVersionAvailable();
			if (newVersion)
			{
				notificationService.ShowNotification("Update available",
					"Click to open the releases page.", "https://github.com/felixse/FluentTerminal/releases");
			}
		}
	}
}