using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

namespace FluentTerminal.SystemTray
{
    public class SystemTrayApplicationContext : ApplicationContext
    {
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

        private void Exit(object sender, EventArgs e)
        {
            Process.Start("flute.exe", "close");
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

        private void ShowSettings(object sender, EventArgs e)
        {
            Process.Start("flute.exe", "settings");
        }
    }
}