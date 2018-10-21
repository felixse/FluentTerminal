﻿using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Settings;
using GalaSoft.MvvmLight;
using System;

namespace FluentTerminal.App.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(ISettingsService settingsService, IDefaultValueProvider defaultValueProvider, IDialogService dialogService,
            ITrayProcessCommunicationService trayProcessCommunicationService, IThemeParserFactory themeParserFactory, ISystemFontService systemFontService,
            IFileSystemService fileSystemService, IStartupTaskService startupTaskService, IUpdateService updateService)
        {
            About = new AboutPageViewModel(settingsService, updateService);
            KeyBindings = new KeyBindingsPageViewModel(settingsService, dialogService, defaultValueProvider, trayProcessCommunicationService);
            General = new GeneralPageViewModel(settingsService, dialogService, defaultValueProvider, startupTaskService);
            Shell = new ProfilesPageViewModel(settingsService, dialogService, defaultValueProvider, fileSystemService);
            Terminal = new TerminalPageViewModel(settingsService, dialogService, defaultValueProvider, systemFontService);
            Themes = new ThemesPageViewModel(settingsService, dialogService, defaultValueProvider, themeParserFactory, fileSystemService);
            Mouse = new MousePageViewModel(settingsService, dialogService, defaultValueProvider);
        }

        public event EventHandler Closed;

        public GeneralPageViewModel General { get; }
        public KeyBindingsPageViewModel KeyBindings { get; }
        public ProfilesPageViewModel Shell { get; }
        public TerminalPageViewModel Terminal { get; }
        public ThemesPageViewModel Themes { get; }
        public MousePageViewModel Mouse { get; }
        public AboutPageViewModel About { get; }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}