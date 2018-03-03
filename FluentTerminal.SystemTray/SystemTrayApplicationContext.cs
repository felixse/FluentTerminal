using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;

namespace FluentTerminal.SystemTray
{
    public class SystemTrayApplicationContext : ApplicationContext
    {
        private AppServiceConnection _connection;
        private readonly NotifyIcon _notifyIcon;

        public SystemTrayApplicationContext()

        {
            var openMenuItem = new MenuItem("Show", new EventHandler(OpenApp));
            var newWindowItem = new MenuItem("New window", new EventHandler(NewWindow));
            var settingsMenuItem = new MenuItem("Show settings", new EventHandler(ShowSettings));
            var exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            openMenuItem.DefaultItem = true;

            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += OpenApp;
            _notifyIcon.Text = "Fluent Terminal";
            _notifyIcon.Icon = Properties.Resources.Square44x44Logo_scale_100;
            _notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { openMenuItem, newWindowItem, settingsMenuItem, exitMenuItem });
            _notifyIcon.Visible = true;
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _connection.ServiceClosed -= Connection_ServiceClosed;
            _connection = null;
        }

        private async void Exit(object sender, EventArgs e)
        {
            var message = new ValueSet
            {
                { "exit", string.Empty }
            };
            await SendMessage(message);
            Application.Exit();
        }

        private void NewWindow(object sender, EventArgs e)
        {
            Process.Start("flute.exe", "new");
        }

        private async void OpenApp(object sender, EventArgs e)
        {
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();
        }

        private async Task SendMessage(ValueSet message)

        {
            if (_connection == null)
            {
                _connection = new AppServiceConnection
                {
                    PackageFamilyName = Package.Current.Id.FamilyName,
                    AppServiceName = "FluentTerminalAppService"
                };
                _connection.ServiceClosed += Connection_ServiceClosed;
                AppServiceConnectionStatus connectionStatus = await _connection.OpenAsync();

                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    MessageBox.Show("Status: " + connectionStatus.ToString());
                    return;
                }
            }
            await _connection.SendMessageAsync(message);
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            Process.Start("flute.exe", "settings");
        }
    }
}