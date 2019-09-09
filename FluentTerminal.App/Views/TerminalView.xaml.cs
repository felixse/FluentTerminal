using FluentTerminal.App.ViewModels;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalView : UserControl
    {
        private ITerminalView _terminalView;

        public TerminalView(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.SearchStarted += OnSearchStarted;
            ViewModel.Activated += OnActivated;
            ViewModel.ThemeChanged += OnThemeChanged;
            ViewModel.OptionsChanged += OnOptionsChanged;
            ViewModel.KeyBindingsChanged += OnKeyBindingsChanged;
            ViewModel.FindNextRequested += OnFindNextRequested;
            ViewModel.FindPreviousRequested += OnFindPreviousRequested;
            InitializeComponent();
            _terminalView = new XtermTerminalView();
            TerminalContainer.Children.Add((UIElement)_terminalView);
            _terminalView.Initialize(ViewModel);
            ViewModel.TerminalView = _terminalView;
            ViewModel.Initialized = true;
        }

        public void DisposalPrepare()
        {
            Bindings.StopTracking();
            TerminalContainer.Children.Remove((UIElement)_terminalView);
            _terminalView = null;

            ViewModel.SearchStarted -= OnSearchStarted;
            ViewModel.Activated -= OnActivated;
            ViewModel.ThemeChanged -= OnThemeChanged;
            ViewModel.OptionsChanged -= OnOptionsChanged;
            ViewModel.KeyBindingsChanged -= OnKeyBindingsChanged;
            ViewModel.FindNextRequested -= OnFindNextRequested;
            ViewModel.FindPreviousRequested -= OnFindPreviousRequested;

            ViewModel = null;
        }

        public TerminalViewModel ViewModel { get; private set; }

        private async void OnActivated(object sender, EventArgs e)
        {
            if (_terminalView != null)
            {
                await _terminalView.FocusTerminal().ConfigureAwait(true);
            }
        }

        private async void OnFindNextRequested(object sender, string e)
        {
            await _terminalView.FindNext(e).ConfigureAwait(true);
        }

        private async void OnFindPreviousRequested(object sender, string e)
        {
            await _terminalView.FindPrevious(e).ConfigureAwait(true);
        }

        private async void OnKeyBindingsChanged(object sender, EventArgs e)
        {
            await _terminalView.ChangeKeyBindings().ConfigureAwait(true);
        }

        private async void OnOptionsChanged(object sender, Models.TerminalOptions e)
        {
            await _terminalView.ChangeOptions(e).ConfigureAwait(true);
        }

        private void OnSearchStarted(object sender, EventArgs e)
        {
            SearchTextBox.Focus(FocusState.Programmatic);
        }

        private void OnSearchTextBoxKeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                ViewModel.CloseSearchPanelCommand.Execute(null);
            }
            else if (e.Key == VirtualKey.Enter)
            {
                ViewModel.FindNextCommand.Execute(null);
            }
        }

        private async void OnThemeChanged(object sender, Models.TerminalTheme e)
        {
            await _terminalView.ChangeTheme(e).ConfigureAwait(true);
        }
    }
}