using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using Windows.System;

namespace FluentTerminal.App.Services.Implementation
{
    internal class DefaultValueProvider : IDefaultValueProvider
    {
        public ApplicationSettings GetDefaultApplicationSettings()
        {
            return new ApplicationSettings
            {
                ConfirmClosingTabs = false,
                ConfirmClosingWindows = false,
                NewTerminalLocation = NewTerminalLocation.Tab
            };
        }

        public KeyBindings GetDefaultKeyBindings()
        {
            return new KeyBindings
            {
                NewTab = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.NewTab,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.T
                    }
                },
                ConfigurableNewTab = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.ConfigurableNewTab,
                        Ctrl = true,
                        Alt = false,
                        Shift = true,
                        Key = (int)VirtualKey.T
                    }
                },
                CloseTab = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.CloseTab,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.W
                    }
                },
                NextTab = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.NextTab,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.Tab
                    }
                },
                PreviousTab = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.PreviousTab,
                        Ctrl = true,
                        Alt = false,
                        Shift = true,
                        Key = (int)VirtualKey.Tab
                    }
                },
                NewWindow = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.NewWindow,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.N
                    }
                },
                ShowSettings = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.ShowSettings,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = 188 // comma
                    }
                },
                Copy = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.Copy,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.C
                    }
                },
                Paste = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.Paste,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.V
                    }
                },
                ToggleWindow = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.ToggleWindow,
                        Ctrl = false,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.Scroll
                    }
                },
                Search = new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = Command.Search,
                        Ctrl = true,
                        Alt = false,
                        Shift = false,
                        Key = (int)VirtualKey.F
                    }
                }
            };
        }

        public Guid GetDefaultShellProfileId()
        {
            return Guid.Parse("813f2298-210a-481a-bdbf-c17bc637a3e2");
        }

        public TerminalOptions GetDefaultTerminalOptions()
        {
            return new TerminalOptions
            {
                BellStyle = BellStyle.None,
                CursorBlink = true,
                CursorStyle = CursorStyle.Block,
                ScrollBarStyle = ScrollBarStyle.Hidden,
                FontFamily = "Consolas",
                FontSize = 13
            };
        }

        public Guid GetDefaultThemeId()
        {
            return Guid.Parse("281e4352-bb50-47b7-a691-2b13830df95e");
        }

        public IEnumerable<ShellProfile> GetPreinstalledShellProfiles()
        {
            return new[]
            {
                new ShellProfile
                {
                    Id = GetDefaultShellProfileId(),
                    Name = "Powershell",
                    Arguments = string.Empty,
                    Location = @"C:\windows\system32\WindowsPowerShell\v1.0\powershell.exe",
                    PreInstalled = true,
                    WorkingDirectory = string.Empty
                },
                new ShellProfile
                {
                    Id = Guid.Parse("ab942a61-7673-4755-9bd8-765aff91d9a3"),
                    Name = "CMD",
                    Arguments = string.Empty,
                    Location = @"C:\Windows\System32\cmd.exe",
                    PreInstalled = true,
                    WorkingDirectory = string.Empty
                }
            };
        }

        public IEnumerable<TerminalTheme> GetPreInstalledThemes()
        {
            var defaultXterm = new TerminalTheme
            {
                Id = GetDefaultThemeId(),
                Author = "xterm.js",
                Name = "Xterm.js Default",
                PreInstalled = true,
                BackgroundOpacity = 0.8,
                Colors = new TerminalColors
                {
                    Black = "#2e3436",
                    Red = "#cc0000",
                    Green = "#4e9a06",
                    Yellow = "#c4a000",
                    Blue = "#3465a4",
                    Magenta = "#75507b",
                    Cyan = "#06989a",
                    White = "#d3d7cf",
                    BrightBlack = "#555753",
                    BrightRed = "#ef2929",
                    BrightGreen = "#8ae234",
                    BrightYellow = "#fce94f",
                    BrightBlue = "#729fcf",
                    BrightMagenta = "#ad7fa8",
                    BrightCyan = "#34e2e2",
                    BrightWhite = "#eeeeec",
                    Foreground = "#ffffff",
                    Background = "#000000",
                    Cursor = "#ffffff",
                    CursorAccent = "#000000",
                    Selection = "rgba(255, 255, 255, 0.3)"
                }
            };

            var powerShell = new TerminalTheme
            {
                Id = Guid.Parse("3571ce1b-31ce-4cf7-ae15-e0bff70c3eea"),
                Author = "Microsoft",
                Name = "PowerShell",
                PreInstalled = true,
                BackgroundOpacity = 0.8,
                Colors = new TerminalColors
                {
                    Black = "#000000",
                    Red = "#800000",
                    Green = "#008000",
                    Yellow = "#EEEDF0",
                    Blue = "#000080",
                    Magenta = "#012456",
                    Cyan = "#008080",
                    White = "#c0c0c0",
                    BrightBlack = "#808080",
                    BrightRed = "#ff0000",
                    BrightGreen = "#00ff00",
                    BrightYellow = "#ffff00",
                    BrightBlue = "#0000ff",
                    BrightMagenta = "#ff00ff",
                    BrightCyan = "#00ffff",
                    BrightWhite = "#ffffff",
                    Foreground = "#ffffff",
                    Background = "#012456",
                    Cursor = "#fedba9",
                    CursorAccent = "#000000",
                    Selection = "#fedba9"
                }
            };

            var homebrew = new TerminalTheme
            {
                Id = Guid.Parse("5f034210-067b-40e2-b9c9-6a25eb6fb8e6"),
                Author = "Hans Kokx",
                Name = "Homebrew",
                PreInstalled = true,
                BackgroundOpacity = 0.8,
                Colors = new TerminalColors
                {
                    Black = "#000000",
                    Red = "#A0160B",
                    Green = "#00AF21",
                    Yellow = "#A1A222",
                    Blue = "#192AB7",
                    Magenta = "#AA2FAE",
                    Cyan = "#12B1BC",
                    White = "#BBB5AF",
                    BrightBlack = "#747876",
                    BrightRed = "#E52213",
                    BrightGreen = "#00D92B",
                    BrightYellow = "#E6E435",
                    BrightBlue = "#283EF9",
                    BrightMagenta = "#EB43E6",
                    BrightCyan = "#15E7E8",
                    BrightWhite = "#E9E9E9",
                    Foreground = "#00D92B",
                    Background = "#000000",
                    Cursor = "#00D92B",
                    CursorAccent = "#000000",
                    Selection = "#4CFFFFFF"
                }
            };

            return new[] { defaultXterm, powerShell, homebrew };
        }
    }
}