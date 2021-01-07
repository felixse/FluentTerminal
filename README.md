<p align="center">
  <img alt="fluent terminal logo" src="./Icons/Icon_no_margin.png" width="100px" />
  <h1 align="center">Fluent Terminal</h1>
</p>

![CI](https://github.com/felixse/FluentTerminal/workflows/CI/badge.svg?branch=master)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=FluentTerminal&metric=alert_status)](https://sonarcloud.io/dashboard?id=FluentTerminal)
[![Gitter chat](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/FluentTerminal)
[![Downloads](https://img.shields.io/github/downloads/felixse/FluentTerminal/total.svg?label=Downloads)](https://github.com/felixse/FluentTerminal/releases/)
[![Release](https://img.shields.io/github/release/felixse/FluentTerminal.svg?label=Release)](https://github.com/felixse/FluentTerminal/releases)
[![Contributors](https://img.shields.io/github/contributors/felixse/FluentTerminal?label=Contributors)](https://github.com/felixse/FluentTerminal/graphs/contributors)

A Terminal Emulator based on UWP and web technologies.


<a href='//www.microsoft.com/store/apps/9p2krlmfxf9t?cid=storebadge&ocid=badge'><img src='store-badge.png' alt='Get it from Microsoft' width="284px" height="104px" /></a>

## Features

- Terminal for PowerShell, CMD, WSL or custom shells
- Built-in support for SSH and [Mosh](https://mosh.org/) connections
- Supports tabs and multiple windows
- Theming and appearance configuration
- Import/Export themes
- Import iTerm themes
- Fullscreen mode
- Editable keybindings
- Search function
- Configure shell profiles to quickly switch between different shells
- Explorer context menu integration (Installation script can be found [here](https://github.com/felixse/FluentTerminal/tree/master/Explorer%20Context%20Menu%20Integration))
- VsCode Extension ([here](https://marketplace.visualstudio.com/items?itemName=Xherdi.fluent-terminal))

## Screenshots

![Terminal window](Screenshots/terminal.png)
![Settings window](Screenshots/settings.png)

## Languages
- Arabic
- Arabic (Iraq)
- Azerbaijani
- Bosnian
- Chinese (Simplified)
- Chinese (Traditional)
- Dutch
- English
- French
- German
- Hebrew
- Hindi
- Indonesian
- Italian
- Japanese
- Korean
- Polish
- Portuguese (Brazilian)
- Romanian
- Russian
- Spanish
- Ukrainian


Sign up to [POEditor](https://poeditor.com/join/project/TfKNIt826z) to help with existing translations or add a new language

## How to install (as an end-user)

### Prerequisite
- You need to update to Fall Creators Update or later.

### Microsoft Store
- Search for Fluent Terminal in the Microsoft Store App or click [here](https://www.microsoft.com/store/apps/9p2krlmfxf9t)

### Chocolatey package manager installation

- Install [Chocolatey](https://chocolatey.org/)
- From an elevated/admin shell, execute `choco install fluent-terminal`

### Scoop package manager installation
- Install Scoop
- Install 7zip and git (to add buckets): `scoop install 7zip git`
- From an elevated/admin shell, execute
```
 scoop bucket add nonportable
 scoop install fluent-terminal-np
 ```

### Manual

- Download and extract the latest [release](https://github.com/felixse/FluentTerminal/releases).
- Double click the *.appxbundle file.

## How to set up a development environment
Please refer to [this Wiki page](https://github.com/felixse/FluentTerminal/wiki/How-to-set-up-a-development-environment)
