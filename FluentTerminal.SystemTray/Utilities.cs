using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Input;

namespace FluentTerminal.SystemTray
{
    public static class Utilities
    {
        private const int FirstDynamicPort = 49151;
        private static readonly List<int> _sentOutPorts = new List<int>();

        public static string GetSshLocation()
        {
            //
            // See https://stackoverflow.com/a/25919981
            //

            string system32Folder;

            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                system32Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Sysnative");
            }
            else
            {
                system32Folder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            }

            return Path.Combine(system32Folder, @"OpenSSH\ssh.exe");
        }

        public static int? GetAvailablePort()
        {
            var usedPorts = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            var connections = properties.GetActiveTcpConnections();
            usedPorts.AddRange(connections.Where(c => c.LocalEndPoint.Port >= FirstDynamicPort).Select(c => c.LocalEndPoint.Port));

            var endPoints = properties.GetActiveTcpListeners();
            usedPorts.AddRange(endPoints.Where(e => e.Port >= FirstDynamicPort).Select(e => e.Port));

            endPoints = properties.GetActiveUdpListeners();
            usedPorts.AddRange(endPoints.Where(e => e.Port >= FirstDynamicPort).Select(e => e.Port));

            usedPorts.AddRange(_sentOutPorts);

            usedPorts.Sort();

            for (var i = FirstDynamicPort; i < UInt16.MaxValue; i++)
            {
                if (!usedPorts.Contains(i))
                {
                    _sentOutPorts.Add(i);
                    return i;
                }
            }
            return null;
        }

        public static Key ExtendVirtualKeyToInputKey(ExtendedVirtualKey key)
        {
            switch (key)
            {
                case ExtendedVirtualKey.Cancel:
                    return Key.Cancel;

                case ExtendedVirtualKey.Back:
                    return Key.Back;

                case ExtendedVirtualKey.Tab:
                    return Key.Tab;

                case ExtendedVirtualKey.Clear:
                    return Key.Clear;

                case ExtendedVirtualKey.Enter:
                    return Key.Enter;

                case ExtendedVirtualKey.Shift:
                    return Key.LeftShift;

                case ExtendedVirtualKey.Menu:
                    return Key.LeftAlt;

                case ExtendedVirtualKey.Pause:
                    return Key.Pause;

                case ExtendedVirtualKey.CapitalLock:
                    return Key.CapsLock;

                case ExtendedVirtualKey.Kana:
                    return Key.KanaMode;

                case ExtendedVirtualKey.Junja:
                    return Key.JunjaMode;

                case ExtendedVirtualKey.Final:
                    return Key.FinalMode;

                case ExtendedVirtualKey.Kanji:
                    return Key.KanjiMode;

                case ExtendedVirtualKey.Escape:
                    return Key.Escape;

                case ExtendedVirtualKey.Convert:
                    return Key.ImeConvert;

                case ExtendedVirtualKey.NonConvert:
                    return Key.ImeNonConvert;

                case ExtendedVirtualKey.Accept:
                    return Key.ImeAccept;

                case ExtendedVirtualKey.ModeChange:
                    return Key.ImeModeChange;

                case ExtendedVirtualKey.Space:
                    return Key.Space;

                case ExtendedVirtualKey.PageUp:
                    return Key.PageUp;

                case ExtendedVirtualKey.PageDown:
                    return Key.PageDown;

                case ExtendedVirtualKey.End:
                    return Key.End;

                case ExtendedVirtualKey.Home:
                    return Key.Home;

                case ExtendedVirtualKey.Left:
                    return Key.Left;

                case ExtendedVirtualKey.Up:
                    return Key.Up;

                case ExtendedVirtualKey.Right:
                    return Key.Right;

                case ExtendedVirtualKey.Down:
                    return Key.Down;

                case ExtendedVirtualKey.Select:
                    return Key.Select;

                case ExtendedVirtualKey.Print:
                    return Key.Print;

                case ExtendedVirtualKey.Execute:
                    return Key.Execute;

                case ExtendedVirtualKey.Snapshot:
                    return Key.Snapshot;

                case ExtendedVirtualKey.Insert:
                    return Key.Insert;

                case ExtendedVirtualKey.Delete:
                    return Key.Delete;

                case ExtendedVirtualKey.Help:
                    return Key.Help;

                case ExtendedVirtualKey.Number0:
                    return Key.D0;

                case ExtendedVirtualKey.Number1:
                    return Key.D1;

                case ExtendedVirtualKey.Number2:
                    return Key.D2;

                case ExtendedVirtualKey.Number3:
                    return Key.D3;

                case ExtendedVirtualKey.Number4:
                    return Key.D4;

                case ExtendedVirtualKey.Number5:
                    return Key.D5;

                case ExtendedVirtualKey.Number6:
                    return Key.D6;

                case ExtendedVirtualKey.Number7:
                    return Key.D7;

                case ExtendedVirtualKey.Number8:
                    return Key.D8;

                case ExtendedVirtualKey.Number9:
                    return Key.D9;

                case ExtendedVirtualKey.A:
                    return Key.A;

                case ExtendedVirtualKey.B:
                    return Key.B;

                case ExtendedVirtualKey.C:
                    return Key.C;

                case ExtendedVirtualKey.D:
                    return Key.D;

                case ExtendedVirtualKey.E:
                    return Key.E;

                case ExtendedVirtualKey.F:
                    return Key.F;

                case ExtendedVirtualKey.G:
                    return Key.G;

                case ExtendedVirtualKey.H:
                    return Key.H;

                case ExtendedVirtualKey.I:
                    return Key.I;

                case ExtendedVirtualKey.J:
                    return Key.J;

                case ExtendedVirtualKey.K:
                    return Key.K;

                case ExtendedVirtualKey.L:
                    return Key.L;

                case ExtendedVirtualKey.M:
                    return Key.M;

                case ExtendedVirtualKey.N:
                    return Key.N;

                case ExtendedVirtualKey.O:
                    return Key.O;

                case ExtendedVirtualKey.P:
                    return Key.P;

                case ExtendedVirtualKey.Q:
                    return Key.Q;

                case ExtendedVirtualKey.R:
                    return Key.R;

                case ExtendedVirtualKey.S:
                    return Key.S;

                case ExtendedVirtualKey.T:
                    return Key.T;

                case ExtendedVirtualKey.U:
                    return Key.U;

                case ExtendedVirtualKey.V:
                    return Key.V;

                case ExtendedVirtualKey.W:
                    return Key.W;

                case ExtendedVirtualKey.X:
                    return Key.X;

                case ExtendedVirtualKey.Y:
                    return Key.Y;

                case ExtendedVirtualKey.Z:
                    return Key.Z;

                case ExtendedVirtualKey.LeftWindows:
                    return Key.LWin;

                case ExtendedVirtualKey.RightWindows:
                    return Key.RWin;

                case ExtendedVirtualKey.Application:
                    return Key.Apps;

                case ExtendedVirtualKey.Sleep:
                    return Key.Sleep;

                case ExtendedVirtualKey.NumberPad0:
                    return Key.NumPad0;

                case ExtendedVirtualKey.NumberPad1:
                    return Key.NumPad1;

                case ExtendedVirtualKey.NumberPad2:
                    return Key.NumPad2;

                case ExtendedVirtualKey.NumberPad3:
                    return Key.NumPad3;

                case ExtendedVirtualKey.NumberPad4:
                    return Key.NumPad4;

                case ExtendedVirtualKey.NumberPad5:
                    return Key.NumPad5;

                case ExtendedVirtualKey.NumberPad6:
                    return Key.NumPad6;

                case ExtendedVirtualKey.NumberPad7:
                    return Key.NumPad7;

                case ExtendedVirtualKey.NumberPad8:
                    return Key.NumPad8;

                case ExtendedVirtualKey.NumberPad9:
                    return Key.NumPad9;

                case ExtendedVirtualKey.Multiply:
                    return Key.Multiply;

                case ExtendedVirtualKey.Add:
                    return Key.Add;

                case ExtendedVirtualKey.Separator:
                    return Key.Separator;

                case ExtendedVirtualKey.Subtract:
                    return Key.Subtract;

                case ExtendedVirtualKey.Decimal:
                    return Key.Decimal;

                case ExtendedVirtualKey.Divide:
                    return Key.Divide;

                case ExtendedVirtualKey.F1:
                    return Key.F1;

                case ExtendedVirtualKey.F2:
                    return Key.F2;

                case ExtendedVirtualKey.F3:
                    return Key.F3;

                case ExtendedVirtualKey.F4:
                    return Key.F4;

                case ExtendedVirtualKey.F5:
                    return Key.F5;

                case ExtendedVirtualKey.F6:
                    return Key.F6;

                case ExtendedVirtualKey.F7:
                    return Key.F7;

                case ExtendedVirtualKey.F8:
                    return Key.F8;

                case ExtendedVirtualKey.F9:
                    return Key.F9;

                case ExtendedVirtualKey.F10:
                    return Key.F10;

                case ExtendedVirtualKey.F11:
                    return Key.F11;

                case ExtendedVirtualKey.F12:
                    return Key.F12;

                case ExtendedVirtualKey.F13:
                    return Key.F13;

                case ExtendedVirtualKey.F14:
                    return Key.F14;

                case ExtendedVirtualKey.F15:
                    return Key.F15;

                case ExtendedVirtualKey.F16:
                    return Key.F16;

                case ExtendedVirtualKey.F17:
                    return Key.F17;

                case ExtendedVirtualKey.F18:
                    return Key.F18;

                case ExtendedVirtualKey.F19:
                    return Key.F19;

                case ExtendedVirtualKey.F20:
                    return Key.F20;

                case ExtendedVirtualKey.F21:
                    return Key.F21;

                case ExtendedVirtualKey.F22:
                    return Key.F22;

                case ExtendedVirtualKey.F23:
                    return Key.F23;

                case ExtendedVirtualKey.F24:
                    return Key.F24;

                case ExtendedVirtualKey.NumberKeyLock:
                    return Key.NumLock;

                case ExtendedVirtualKey.Scroll:
                    return Key.Scroll;

                case ExtendedVirtualKey.LeftShift:
                    return Key.LeftShift;

                case ExtendedVirtualKey.RightShift:
                    return Key.RightShift;

                case ExtendedVirtualKey.LeftControl:
                    return Key.LeftCtrl;

                case ExtendedVirtualKey.RightControl:
                    return Key.RightCtrl;

                case ExtendedVirtualKey.LeftMenu:
                    return Key.LeftAlt;

                case ExtendedVirtualKey.RightMenu:
                    return Key.RightAlt;

                case ExtendedVirtualKey.GoBack:
                    return Key.BrowserBack;

                case ExtendedVirtualKey.GoForward:
                    return Key.BrowserForward;

                case ExtendedVirtualKey.Refresh:
                    return Key.BrowserRefresh;

                case ExtendedVirtualKey.Stop:
                    return Key.BrowserStop;

                case ExtendedVirtualKey.Search:
                    return Key.BrowserSearch;

                case ExtendedVirtualKey.Favorites:
                    return Key.BrowserFavorites;

                case ExtendedVirtualKey.GoHome:
                    return Key.BrowserHome;

                case ExtendedVirtualKey.VolumeMute:
                    return Key.VolumeMute;

                case ExtendedVirtualKey.VolumeDown:
                    return Key.VolumeDown;

                case ExtendedVirtualKey.VolumeUp:
                    return Key.VolumeUp;

                case ExtendedVirtualKey.MediaNext:
                    return Key.MediaNextTrack;

                case ExtendedVirtualKey.MediaPrevious:
                    return Key.MediaPreviousTrack;

                case ExtendedVirtualKey.MediaStop:
                    return Key.MediaStop;

                case ExtendedVirtualKey.MediaPlayPause:
                    return Key.MediaPlayPause;

                case ExtendedVirtualKey.Plus:
                    return Key.OemPlus;

                case ExtendedVirtualKey.Comma:
                    return Key.OemComma;

                case ExtendedVirtualKey.Minus:
                    return Key.OemMinus;

                case ExtendedVirtualKey.Period:
                    return Key.OemPeriod;

                case ExtendedVirtualKey.OEM_1:
                    return Key.Oem1;

                case ExtendedVirtualKey.OEM_2:
                    return Key.Oem1;

                case ExtendedVirtualKey.OEM_3:
                    return Key.Oem3;

                case ExtendedVirtualKey.OEM_5:
                    return Key.Oem5;

                case ExtendedVirtualKey.OEM_6:
                    return Key.Oem6;

                case ExtendedVirtualKey.OEM_7:
                    return Key.Oem7;

                case ExtendedVirtualKey.OEM_102:
                    return Key.Oem102;

                default:
                    return Key.None;
            }
        }

        private static void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Users\peske\Desktop\f.log", true))
                writer.WriteLine(message);
        }
    }
}