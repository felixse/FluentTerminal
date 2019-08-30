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
            IFileSystemService fileSystemService, IStartupTaskService startupTaskService, IUpdateService updateService, IApplicationView applicationView, 
            IApplicationLanguageService applicationLanguageService, ApplicationDataContainers containers)
        {
            About = new AboutPageViewModel(updateService, applicationView);
            KeyBindings = new KeyBindingsPageViewModel(settingsService, dialogService, trayProcessCommunicationService);
            General = new GeneralPageViewModel(settingsService, dialogService, defaultValueProvider, startupTaskService, applicationLanguageService, trayProcessCommunicationService, fileSystemService);
            Profiles = new ProfilesPageViewModel(settingsService, dialogService, defaultValueProvider, fileSystemService, applicationView);
            Terminal = new TerminalPageViewModel(settingsService, dialogService, defaultValueProvider, systemFontService);
            Themes = new ThemesPageViewModel(settingsService, dialogService, defaultValueProvider, themeParserFactory, fileSystemService);
            Mouse = new MousePageViewModel(settingsService, dialogService, defaultValueProvider);
            SshProfiles = new SshProfilesPageViewModel(settingsService, dialogService, fileSystemService,
                applicationView, trayProcessCommunicationService, containers.HistoryContainer);
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