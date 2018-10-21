﻿using FluentTerminal.App.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class AboutPageViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;
        
        public RelayCommand CheckForUpdatesCommand { get; }

        public AboutPageViewModel(ISettingsService settingsService, IUpdateService updateService)
        {
            _settingsService = settingsService;
            _updateService = updateService;

            CheckForUpdatesCommand = new RelayCommand(() => _updateService.CheckForUpdate(true));
        }

        public string GetCurrentVersion()
        {
            var version = _updateService.GetCurrentVersion();
            return ConvertVersionToString(version);
        }

        public string GetLatestVersion()
        {
            var version = _updateService.GetLatestVersion();
            return ConvertVersionToString(version);
        }

        private string ConvertVersionToString(Version version)
        {
            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}