# Fluent Terminal

A Terminal Emulator based on UWP and web technologies.

[![Build status](https://ci.appveyor.com/api/projects/status/4r429bv594fxkygd?svg=true)](https://ci.appveyor.com/project/felixse/fluentterminal)
[![Gitter chat](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/FluentTerminal)

## Features

- Terminal for PowerShell, CMD, (default) WSL or arbitrary custom shells
  - Support for shell arguments allows for native support of remote shells through SSH or other remote-access CLI tools.
- Supports tabs and multiple windows
- Themes and appearance configuration
- Editable keybindings
- Search function
- Configure shell profiles to quickly switch between different shells
- Explorer context menu integration (Installation script can be found [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration))

## Screenshots

![Terminal window](Screenshots/Terminal.png)
![Settings window](Screenshots/Settings.png)

## Up Next

- Split screen support
- Full screen mode
- Import/Export themes

## How to install

- activate the developer mode as described [here](https://docs.microsoft.com/en-US/windows/uwp/get-started/enable-your-device-for-development)
- Install the *.cer file into `Local Machine` -> `Trusted Root Certification Authorities`
- double click the *.appxbundle
- Optional: Install Context menu integration from [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration))

## How to build

Build the Client first, or whenever edited by running `npm run build` in FluentTerminal.Client  
Everything else is part of the solution.

- Grab the [latest NodeJS 8.x installer](https://nodejs.org/en/download/), and install ensuring that both NPM and "install into system PATH" are checked during the install process.
- Grab the [latest Visual Studio 2017 Community installer](https://www.visualstudio.com/downloads/) and install (See next section)
- [Enable developer mode in Windows 10](https://docs.microsoft.com/en-US/windows/uwp/get-started/enable-your-device-for-development)

### Visual Studio 2017 Configuration

- Under "Workloads" select:
  - Universal Windows Platform development
  - Node.js development (Probably not required, given we installed Node.js separately)
- Under individual components, you'll need to install the following:
  - Git for Windows
  - .NET Core runtime
  - .NET 4.7.1 Targeting Pack
  - Windows SDK 16299 for UWP: C#, VB, JS

### The First Build

- Before opening the solution, go into the `FluentTerminal.Client` folder and run:
  - `npm install` (Ignore the warnings related to `fluent-terminal-client`)
  - `npm run build`
- When the solution is first opened, it will help to set the architecture to x64 for testing, as we didn't install the ARM cross-compiling stuff for VS2017.
- When first built, Visual studio will spend a significant amount of time resolving dependencies, and fetching additional components vai nuGet. This is normal.
  - If you have done the steps so far correctly, and the build is left as a debug build, then the solution should build successfully should launch correctly when you press F5.

### Subsequent Builds

- If you change the `FluentTerminal.Client` contents, you'll need to re-run the `npm run build` (and `npm install` if you change the dependencies).
