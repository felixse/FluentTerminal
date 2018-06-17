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

                foreach (Command command in Enum.GetValues(typeof(Command)))
                {
                    // Don't enumerate explicit keybinding enums that are in the range of a profile shortcut
                    // since they won't be directly assigned to in the KeyBindings settings panels.
                    if (command < Command.ShellProfileShortcut)
                    {
                        AddCommandMenu.Items.Add(new MenuFlyoutItem
                        {
                            Text = EnumHelper.GetEnumDescription(command),
                            Command = ViewModel.AddCommand,
                            CommandParameter = command
                        });
                    }
                }

                // Add all of the shells we know of.
                foreach (var shellProfile in viewModel.ShellProfiles)
                {
                    AddCommandMenu.Items.Add(new MenuFlyoutItem
                    {
                        Text = shellProfile.Name + " Shortcut",
                        Command = ViewModel.AddCommand,
                        CommandParameter = shellProfile.KeyBindingCommand
                    });
                }
            }
        }
    }
}
