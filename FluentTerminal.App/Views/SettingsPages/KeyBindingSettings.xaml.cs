using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models.Enums;
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
                foreach (var value in Enum.GetValues(typeof(Command)))
                {
                    Command command = (Command)value;
                    AddCommandMenu.Items.Add(new MenuFlyoutItem
                    {
                        Text = I18N.Translate($"{nameof(Command)}.{command}"),
                        Command = ViewModel.AddCommand,
                        CommandParameter = command.ToString()
                    });
                }
            }
        }
    }
}
