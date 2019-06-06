using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;

namespace FluentTerminal.App.Services.Implementation
{
    public class DefaultValueProvider : IDefaultValueProvider
    {
        public ApplicationSettings GetDefaultApplicationSettings()
        {
            return new ApplicationSettings
            {
                ConfirmClosingTabs = false,
                ConfirmClosingWindows = false,
                UnderlineSelectedTab = false,
                InactiveTabColorMode = InactiveTabColorMode.Background,
                NewTerminalLocation = NewTerminalLocation.Tab,
                TabsPosition = TabsPosition.Top,
                CopyOnSelect = false,
                MouseMiddleClickAction = MouseAction.None,
                MouseRightClickAction = MouseAction.ContextMenu,
                AlwaysShowTabs = false,
                ShowNewOutputIndicator = false,
                EnableTrayIcon = true,
                ShowCustomTitleInTitlebar = true
            };
        }

        public ICollection<KeyBinding> GetDefaultKeyBindings(Command command)
        {
            switch (command)
            {
                case Command.ToggleWindow:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.ToggleWindow),
                        Key = (int)ExtendedVirtualKey.Scroll
                    }
                };

                case Command.NextTab:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.NextTab),
                        Ctrl = true,
                        Key = (int)ExtendedVirtualKey.Tab
                    }
                };

                case Command.PreviousTab:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.PreviousTab),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.Tab
                    }
                };

                case Command.SwitchToTerm1:
                case Command.SwitchToTerm2:
                case Command.SwitchToTerm3:
                case Command.SwitchToTerm4:
                case Command.SwitchToTerm5:
                case Command.SwitchToTerm6:
                case Command.SwitchToTerm7:
                case Command.SwitchToTerm8:
                case Command.SwitchToTerm9:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = command.ToString(),
                        Alt = true,
                        Key = (int)ExtendedVirtualKey.Number1 + (command - Command.SwitchToTerm1)
                    }
                };

                case Command.NewTab:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.NewTab),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.T
                    }
                };

                case Command.ConfigurableNewTab:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.ConfigurableNewTab),
                        Ctrl = true,
                        Alt = true,
                        Key = (int)ExtendedVirtualKey.T
                    }
                };

                case Command.NewSshTab:
                    return new List<KeyBinding>
                    {
                        new KeyBinding
                        {
                            Command = nameof(Command.NewSshTab),
                            Ctrl = true,
                            Shift = true,
                            Key = (int)ExtendedVirtualKey.Y
                        }
                    };

                case Command.ChangeTabTitle:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.ChangeTabTitle),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.R
                    }
                };

                case Command.CloseTab:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.CloseTab),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.W
                    }
                };

                case Command.NewWindow:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.NewWindow),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.N
                    }
                };

                case Command.ConfigurableNewWindow:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.ConfigurableNewWindow),
                        Ctrl = true,
                        Alt = true,
                        Key = (int)ExtendedVirtualKey.N
                    }
                };

                case Command.ShowSettings:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.ShowSettings),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.Comma
                    }
                };

                case Command.Copy:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.Copy),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.C
                    }
                };

                case Command.Paste:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.Paste),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.V
                    }
                };


                case Command.PasteWithoutNewlines:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.PasteWithoutNewlines),
                        Ctrl = true,
                        Alt = true,
                        Key = (int)ExtendedVirtualKey.V
                    }
                };

                case Command.Search:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.Search),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.F
                    }
                };

                case Command.ToggleFullScreen:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.ToggleFullScreen),
                        Alt = true,
                        Key = (int)ExtendedVirtualKey.Enter
                    },
                    new KeyBinding
                    {
                        Command = nameof(Command.ToggleFullScreen),
                        Key = (int)ExtendedVirtualKey.F11
                    }
                };

                case Command.SelectAll:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.SelectAll),
                        Ctrl = true,
                        Shift = true,
                        Key = (int)ExtendedVirtualKey.A
                    }
                };

                case Command.Clear:
                    return new List<KeyBinding>
                {
                    new KeyBinding
                    {
                        Command = nameof(Command.Clear),
                        Ctrl = true,
                        Alt = true,
                        Key = (int)ExtendedVirtualKey.L
                    }
                };

                case Command.SavedSshNewTab:
                    return new List<KeyBinding>
                    {
                        new KeyBinding
                        {
                            Command = nameof(Command.SavedSshNewTab),
                            Ctrl = true,
                            Alt = true,
                            Key = (int)ExtendedVirtualKey.Y
                        }
                    };

                case Command.SavedSshNewWindow:
                    return new List<KeyBinding>
                    {
                        new KeyBinding
                        {
                            Command = nameof(Command.SavedSshNewWindow),
                            Ctrl = true,
                            Alt = true,
                            Shift = true,
                            Key = (int)ExtendedVirtualKey.Z
                        }
                    };

                case Command.NewSshWindow:
                    return new List<KeyBinding>
                    {
                        new KeyBinding
                        {
                            Command = nameof(Command.NewSshWindow),
                            Ctrl = true,
                            Alt = true,
                            Shift = true,
                            Key = (int)ExtendedVirtualKey.Y
                        }
                    };


                    
            }

            return null;
        }

        public Guid GetDefaultShellProfileId()
        {
            return Guid.Parse("813f2298-210a-481a-bdbf-c17bc637a3e2");
        }

        public IEnumerable<TabTheme> GetDefaultTabThemes()
        {
            return new[]
            {
                new TabTheme
                {
                    Id = 0,
                    Name = I18N.Translate("TabTheme.None"),
                    BackgroundOpacity = double.NaN,
                    BackgroundPointerOverOpacity = double.NaN,
                    BackgroundPressedOpacity = double.NaN,
                    BackgroundSelectedOpacity = double.NaN,
                    BackgroundSelectedPointerOverOpacity = double.NaN,
                    BackgroundSelectedPressedOpacity = double.NaN
                },
                new TabTheme
                {
                    Id = 1,
                    Name = I18N.Translate("TabTheme.Red"),
                    Color = "#E81123"
                },
                new TabTheme
                {
                    Id = 2,
                    Name = I18N.Translate("TabTheme.Green"),
                    Color = "#10893E"
                },
                new TabTheme
                {
                    Id = 3,
                    Name = I18N.Translate("TabTheme.Blue"),
                    Color = "#0078D7"
                },
                new TabTheme
                {
                    Id = 4,
                    Name = I18N.Translate("TabTheme.Purple"),
                    Color = "#881798"
                },
                new TabTheme
                {
                    Id = 5,
                    Name = I18N.Translate("TabTheme.Orange"),
                    Color = "#FF8C00"
                },
                new TabTheme
                {
                    Id = 6,
                    Name = I18N.Translate("TabTheme.Teal"),
                    Color = "#00B7C3"
                }
            };
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
                FontSize = 13,
                BoldText = false,
                BackgroundOpacity = 0.8,
                Padding = 12,
                ScrollBackLimit = 1000
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
                    WorkingDirectory = string.Empty,
                    LineEndingTranslation = LineEndingStyle.DoNotModify,
                    UseConPty = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8), // Windows 10 1903+
                    KeyBindings = new []
                    {
                        new KeyBinding
                        {
                            Command = GetDefaultShellProfileId().ToString(),
                            Ctrl=true,
                            Alt=true,
                            Shift=false,
                            Meta=false,
                            Key=(int)ExtendedVirtualKey.Number1
                        }
                    }
                },
                new ShellProfile
                {
                    Id = Guid.Parse("ab942a61-7673-4755-9bd8-765aff91d9a3"),
                    Name = "CMD",
                    Arguments = string.Empty,
                    Location = @"C:\Windows\System32\cmd.exe",
                    PreInstalled = true,
                    WorkingDirectory = string.Empty,
                    LineEndingTranslation = LineEndingStyle.DoNotModify,
                    UseConPty = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7), // Windows 10 1809+
                    KeyBindings = new []
                    {
                        new KeyBinding
                        {
                            Command = "ab942a61-7673-4755-9bd8-765aff91d9a3",
                            Ctrl=true,
                            Alt=true,
                            Shift=false,
                            Meta=false,
                            Key=(int)ExtendedVirtualKey.Number2
                        }
                    }
                },
                new ShellProfile
                {
                    Id= Guid.Parse("e5785ad6-584f-40cb-bdcd-d5b3b3953e7f"),
                    Name = "WSL",
                    Arguments = string.Empty,
                    Location = @"C:\windows\system32\wsl.exe",
                    PreInstalled = true,
                    WorkingDirectory = string.Empty,
                    LineEndingTranslation = LineEndingStyle.ToLF,
                    UseConPty = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7), // Windows 10 1809+
                    KeyBindings = new []
                    {
                        new KeyBinding
                        {
                            Command = "e5785ad6-584f-40cb-bdcd-d5b3b3953e7f",
                            Ctrl=true,
                            Alt=true,
                            Shift=false,
                            Meta=false,
                            Key=(int)ExtendedVirtualKey.Number3
                        }
                    }
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
                    Selection = "rgba(254, 219, 169, 0.3)"
                }
            };

            var homebrew = new TerminalTheme
            {
                Id = Guid.Parse("5f034210-067b-40e2-b9c9-6a25eb6fb8e6"),
                Author = "Hans Kokx",
                Name = "Homebrew",
                PreInstalled = true,
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

            var tomorrow = new TerminalTheme
            {
                Id = Guid.Parse("0cbbf805-fa86-4be5-ac51-75f3054dcc3a"),
                Author = "Chris Kempson",
                Name = "Tomorrow",
                PreInstalled = true,
                Colors = new TerminalColors
                {
                    Foreground = "#4D4D4C",
                    Background = "#FFFFFF",
                    Cursor = "#4D4D4C",
                    CursorAccent = "#FFFFFF",
                    Selection = "rgba(214, 214, 214, 0.5)",
                    Black = "#000000",
                    Red = "#C82829",
                    Green = "#718C00",
                    Yellow = "#EAB700",
                    Blue = "#4271AE",
                    Magenta = "#8959A8",
                    Cyan = "#3E999F",
                    White = "#FFFFFF",
                    BrightBlack = "#000000",
                    BrightRed = "#C82829",
                    BrightGreen = "#718C00",
                    BrightYellow = "#EAB700",
                    BrightBlue = "#4271AE",
                    BrightMagenta = "#8959A8",
                    BrightCyan = "#3E999F",
                    BrightWhite = "#FFFFFF"
                }
            };

            var ubuntu = new TerminalTheme
            {
                Id = Guid.Parse("32519543-bf94-4db6-92a1-7bcec3966c82"),
                Author = "Canonical",
                Name = "Ubuntu Bash",
                PreInstalled = true,
                Colors = new TerminalColors
                {
                    Foreground = "#EEEEEE",
                    Background = "#300A24",
                    Cursor = "#CFF5DB",
                    CursorAccent = "#000000",
                    Selection = "#4DABEDC1",
                    Black = "#300A24",
                    Red = "#CC0000",
                    Green = "#4E9A06",
                    Yellow = "#C4A000",
                    Blue = "#3465A4",
                    Magenta = "#75507B",
                    Cyan = "#06989A",
                    White = "#D3D7CF",
                    BrightBlack = "#554E53",
                    BrightRed = "#EF2929",
                    BrightGreen = "#8AE234",
                    BrightYellow = "#FCE94F",
                    BrightBlue = "#729FCF",
                    BrightMagenta = "#AD7FA8",
                    BrightCyan = "#34E2E2",
                    BrightWhite = "#EEEEEE"
                }
            };

            return new[] { defaultXterm, powerShell, homebrew, tomorrow, ubuntu };
        }
    }
}
