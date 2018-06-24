
[![Build status](https://ci.appveyor.com/api/projects/status/4r429bv594fxkygd?svg=true)](https://ci.appveyor.com/project/felixse/fluentterminal)
[![Gitter chat](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/FluentTerminal)

# Fluent Terminal
A Terminal Emulator based on UWP and web technologies.

## Features:
- Terminal for PowerShell, CMD, WSL, or custom shells
- Supports tabs and multiple windows
- Theming and appearance configuration
- Editable keybindings
- Search function
- Configure shell profiles to quickly switch between different shells
- Explorer context menu integration (Installation script can be found [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration))

## Screenshots:
![Terminal window](Screenshots/Terminal.png)
![Settings window](Screenshots/Settings.png)

## Up Next:
- Split screen support
- Full screen mode
- Import/Export themes

## How to install
- [Enable sideloading apps ](https://www.windowscentral.com/how-enable-windows-10-sideload-apps-outside-store
)
  - Alternatively, [enable developer mode](https://docs.microsoft.com/en-US/windows/uwp/get-started/enable-your-device-for-development) if you plan to do UWP app development. **For most users that do not intend to do UWP app development, developer mode will not be necessary.**
- Install the *.cer file into `Local Machine` -> `Trusted Root Certification Authorities`
- double click the *.appxbundle
- If you did not enable developer mode, you'll have to run the following Powershell snippet in an Administrative/elevated Powershell prompt (addresses issues such as the one described in [#43](https://github.com/felixse/FluentTerminal/issues/43))
  ```powershell
  $packageFamily = (Get-AppxPackage | where { $_.Name -eq "53621FSApps.FluentTerminal" }).PackageFamilyName
  CheckNetIsolation.exe LoopbackExempt -a -n="$packageFamily"
  ```
- **Optional:** Install Context menu integration from [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration))

## How to build:
Build the Client first, or whenever edited by running `npm run build` in FluentTerminal.Client  
Everything else is part of the solution.
