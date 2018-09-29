using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class KeyBindingSettings : Page
    {
        public KeyBindingsPageViewModel ViewModel { get; private set; }

        public KeyBindingSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is KeyBindingsPageViewModel viewModel)
            {
                ViewModel = viewModel;

                // Add the commands corresponding to all of the application's static commands
                foreach (var value in Enum.GetValues(typeof(AppCommand)))
                {
                    AppCommand command = (AppCommand)value;
                    AddCommandMenu.Items.Add(new MenuFlyoutItem
                    {
                        Text = EnumHelper.GetEnumDescription(command),
                        Command = ViewModel.AddCommand,
                        CommandParameter = (ICommand)command
                    });
                }
            }
        }
    }
}
