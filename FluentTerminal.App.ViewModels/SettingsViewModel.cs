using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Settings;
using GalaSoft.MvvmLight;
using System;

namespace FluentTerminal.App.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(ISettingsService settingsService, IDefaultValueProvider defaultValueProvider, IDialogService dialogService,
            ITrayProcessCommunicationService trayProcessCommunicationService, IThemeParserFactory themeParserFactory, ISystemFontService systemFontService,
            IFileSystemService fileSystemService, IStartupTaskService startupTaskService, IUpdateService updateService, IApplicationView applicationView, IApplicationLanguageService applicationLanguageService)
        {
            About = new AboutPageViewModel(settingsService, updateService, applicationView);
            KeyBindings = new KeyBindingsPageViewModel(settingsService, dialogService, defaultValueProvider, trayProcessCommunicationService);
            General = new GeneralPageViewModel(settingsService, dialogService, defaultValueProvider, startupTaskService, applicationLanguageService);
            Profiles = new ProfilesPageViewModel(settingsService, dialogService, defaultValueProvider, fileSystemService, applicationView);
            Terminal = new TerminalPageViewModel(settingsService, dialogService, defaultValueProvider, systemFontService);
            Themes = new ThemesPageViewModel(settingsService, dialogService, defaultValueProvider, themeParserFactory, fileSystemService);
            Mouse = new MousePageViewModel(settingsService, dialogService, defaultValueProvider);
            SshProfiles = new SshProfilesPageViewModel(settingsService, dialogService, fileSystemService, applicationView, trayProcessCommunicationService);
        }

        public event EventHandler Closed;
        public event EventHandler AboutPageRequested;

        public GeneralPageViewModel General { get; }
        public KeyBindingsPageViewModel KeyBindings { get; }
        public ProfilesPageViewModel Profiles { get; }
        public TerminalPageViewModel Terminal { get; }
        public ThemesPageViewModel Themes { get; }
        public MousePageViewModel Mouse { get; }
        public AboutPageViewModel About { get; }
        public SshProfilesPageViewModel SshProfiles { get; }

        public void NavigateToAboutPage()
        {
            AboutPageRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}