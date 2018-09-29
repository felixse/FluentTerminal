using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

namespace FluentTerminal.SystemTray.Services
{
    public class ToggleWindowService : IDisposable
    {
        private const int SW_MINIMIZE = 6;
        private readonly HotKeyManager _hotKeyManager;
        private readonly INotificationService _notificationService;
        private readonly Dispatcher _dispatcher;
        private bool _disposedValue;
        private readonly List<HotKey> _hotKeys;

        public ToggleWindowService(Dispatcher dispatcher, HotKeyManager hotKeyManager, INotificationService notificationService, ISettingsService settingsService)
        {
            _dispatcher = dispatcher;
            _notificationService = notificationService;
            _hotKeyManager = hotKeyManager;
            _hotKeys = new List<HotKey>();
            _hotKeyManager.KeyPressed += OnKeyPressed;

            var keyBindings = settingsService.GetKeyBindings()[AppCommand.ToggleWindow];
            SetHotKeys(keyBindings);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void SetHotKeys(IEnumerable<KeyBinding> keyBindings)
        {
            foreach (var hotKey in _hotKeys)
            {
                _dispatcher.Invoke(() =>
                {
                    _hotKeyManager.Unregister(hotKey);
                });
            }

            _hotKeys.Clear();

            foreach (var keyBinding in keyBindings)
            {
                try
                {
                    var key = Utilities.ExtendVirtualKeyToInputKey((ExtendedVirtualKey)keyBinding.Key);
                    var modifiers = ModifierKeys.None;
                    if (keyBinding.Alt)
                    {
                        modifiers |= ModifierKeys.Alt;
                    }
                    if (keyBinding.Ctrl)
                    {
                        modifiers |= ModifierKeys.Control;
                    }
                    if (keyBinding.Shift)
                    {
                        modifiers |= ModifierKeys.Shift;
                    }
                    if (keyBinding.Meta)
                    {
                        modifiers |= ModifierKeys.Windows;
                    }

                    var hotKey = new HotKey(key, modifiers);

                    _dispatcher.Invoke(() =>
                    {
                        _hotKeyManager.Register(hotKey);
                    });

                    _hotKeys.Add(hotKey);
                }
                catch (Exception)
                {
                    _notificationService.ShowNotification("Error", "Failed to register the following hotkey: " + GetKeyBindingRepresentation(keyBinding));
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _hotKeyManager?.Dispose();
                }

                _disposedValue = true;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static string GetKeyBindingRepresentation(KeyBinding keyBinding)
        {
            var keys = new List<string>();
            if (keyBinding.Ctrl)
            {
                keys.Add("Ctrl");
            }
            if (keyBinding.Alt)
            {
                keys.Add("Alt");
            }
            if (keyBinding.Shift)
            {
                keys.Add("Shift");
            }

            keys.Add(((ExtendedVirtualKey)keyBinding.Key).ToString());

            return string.Join(" + ", keys);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private string GetActiveProcessFileName()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                var process = Process.GetProcessById((int)pid);
                return process.MainWindowTitle;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async void OnKeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (GetActiveProcessFileName().EndsWith("Fluent Terminal"))
            {
                var hwnd = GetForegroundWindow();
                ShowWindow(hwnd, SW_MINIMIZE);
            }
            else
            {
                IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
                await appListEntries.First().LaunchAsync();
            }
        }
    }
}