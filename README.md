
[![Build status](https://ci.appveyor.com/api/projects/status/4r429bv594fxkygd/branch/master?svg=true)](https://ci.appveyor.com/project/felixse/fluentterminal/branch/master)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=FluentTerminal&metric=alert_status)](https://sonarcloud.io/dashboard?id=FluentTerminal)
[![Gitter chat](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/FluentTerminal)

# Fluent Terminal
A Terminal Emulator based on UWP and web technologies.

## Features:
- Terminal for PowerShell, CMD, WSL or custom shells
- Supports tabs and multiple windows
- Theming and appearance configuration
- Import/Export themes
- Import iTerm themes
- Fullscreen mode
- Editable keybindings
- Search function
- Configure shell profiles to quickly switch between different shells
- Explorer context menu integration (Installation script can be found [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration))

## Screenshots:
![Terminal window](Screenshots/terminal.jpg)
![Settings window](Screenshots/settings.jpg)

## Up Next:
- Launch shell profile with a custom defined keybinding
- Copy&Paste options
- Improved tabs
- Split screen support

## How to install
- [Enable sideloading apps ](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store
)
  - Alternatively, [enable developer mode](https://docs.microsoft.com/en-US/windows/uwp/get-started/enable-your-device-for-development) if you plan to do UWP app development. **For most users that do not intend to do UWP app development, developer mode will not be necessary.**
- Install the *.cer file into `Local Machine` -> `Trusted Root Certification Authorities`
- double click the *.appxbundle
- **Optional:** Install Context menu integration from [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration)

## How to build:
Build the Client first, or whenever edited by running `npm run build` in FluentTerminal.Client  
Everything else is part of the solution.
