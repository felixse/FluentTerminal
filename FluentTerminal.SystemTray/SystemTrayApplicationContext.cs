using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;

namespace FluentTerminal.SystemTray
{
    public class SystemTrayApplicationContext : ApplicationContext
    {
        private AppServiceConnection connection = null;
        private HotKeyManager hotKeyManager = null;
        private NotifyIcon notifyIcon = null;

        public SystemTrayApplicationContext()

        {
            var openMenuItem = new MenuItem("Show", new EventHandler(OpenApp));
            var newWindowItem = new MenuItem("New Window", new EventHandler(NewWindow));
            var settingsMenuItem = new MenuItem("Show Settings", new EventHandler(ShowSettings));
            var exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            openMenuItem.DefaultItem = true;

            notifyIcon = new NotifyIcon();
            notifyIcon.DoubleClick += OpenApp;
            notifyIcon.Text = "Fluent Terminal";
            notifyIcon.Icon = Properties.Resources.Square44x44Logo_scale_100;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { openMenuItem, newWindowItem, settingsMenuItem, exitMenuItem });
            notifyIcon.Visible = true;

            hotKeyManager = new HotKeyManager();
            hotKeyManager.Register(Key.Scroll, ModifierKeys.None);
            hotKeyManager.KeyPressed += HotkeyManager_KeyPressed;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        protected override void Dispose(bool disposing)
        {
            hotKeyManager.Dispose();
            base.Dispose(disposing);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            connection.ServiceClosed -= Connection_ServiceClosed;
            connection = null;
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

        private string GetActiveProcessFileName()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                return p.MainWindowTitle;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void HotkeyManager_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.Scroll)
            {
                if (GetActiveProcessFileName() == "Fluent Terminal")
                {
                    SendKeys.Send("%{Tab}");
                }
                else
                {
                    OpenApp(this, EventArgs.Empty);
                }
            }
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
            if (connection == null)
            {
                connection = new AppServiceConnection
                {
                    PackageFamilyName = Package.Current.Id.FamilyName,
                    AppServiceName = "FluentTerminalAppService"
                };
                connection.ServiceClosed += Connection_ServiceClosed;
                AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();

                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    MessageBox.Show("Status: " + connectionStatus.ToString());
                    return;
                }
            }
            await connection.SendMessageAsync(message);
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            Process.Start("flute.exe", "settings");
        }
    }
}