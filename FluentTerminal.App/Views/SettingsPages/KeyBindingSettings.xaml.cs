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

                foreach (var value in Enum.GetValues(typeof(Command)))
                {
                    var command = (Command)value;
                    AddCommandMenu.Items.Add(new MenuFlyoutItem
                    {
                        Text = EnumHelper.GetEnumDescription(command),
                        Command = ViewModel.AddCommand,
                        CommandParameter = command
                    });
                }
            }
        }
    }
}
