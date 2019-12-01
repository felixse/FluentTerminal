using FluentTerminal.App.ViewModels;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.Models.Messages;
using GalaSoft.MvvmLight.Messaging;
using FluentTerminal.Models;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using FluentTerminal.App.Utilities;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class TerminalView : UserControl
    {
        private ITerminalView _terminalView;

        public TerminalView(TerminalViewModel viewModel)
        {
            Messenger.Default.Register<KeyBindingsChangedMessage>(this, OnKeyBindingsChanged);
            Messenger.Default.Register<TerminalOptionsChangedMessage>(this, OnTerminalOptionsChanged);

            ViewModel = viewModel;
            ViewModel.SearchStarted += OnSearchStarted;
            ViewModel.Activated += OnActivated;
            ViewModel.ThemeChanged += OnThemeChanged;
            ViewModel.FindNextRequested += OnFindNextRequested;
            ViewModel.FindPreviousRequested += OnFindPreviousRequested;
            InitializeComponent();
            _terminalView = new XtermTerminalView();
            TerminalContainer.Children.Add((UIElement)_terminalView);
            _terminalView.Initialize(ViewModel);
            ViewModel.TerminalView = _terminalView;
            ViewModel.Initialized = true;

            SetGridBackgroundTheme(ViewModel.TerminalTheme);
        }

        public void DisposalPrepare()
        {
            Bindings.StopTracking();
            TerminalContainer.Children.Remove((UIElement)_terminalView);
            _terminalView?.Dispose();
            _terminalView = null;

            Messenger.Default.Unregister(this);

            ViewModel.SearchStarted -= OnSearchStarted;
            ViewModel.Activated -= OnActivated;
            ViewModel.ThemeChanged -= OnThemeChanged;
            ViewModel.FindNextRequested -= OnFindNextRequested;
            ViewModel.FindPreviousRequested -= OnFindPreviousRequested;

            ViewModel = null;
        }

        public TerminalViewModel ViewModel { get; private set; }

        private void OnActivated(object sender, EventArgs e)
        {
            _terminalView?.FocusTerminal();
        }

        private void OnFindNextRequested(object sender, string e)
        {
            _terminalView.FindNext(e);
        }

        private void OnFindPreviousRequested(object sender, string e)
        {
            _terminalView.FindPrevious(e);
        }

        private void OnKeyBindingsChanged(KeyBindingsChangedMessage message)
        {
            _terminalView.ChangeKeyBindings();
        }

        private void OnTerminalOptionsChanged(TerminalOptionsChangedMessage message)
        {
            _terminalView.ChangeOptions(message.TerminalOptions);
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

        private async void OnThemeChanged(object sender, TerminalTheme e)
        {
            await _terminalView.ChangeTheme(e).ConfigureAwait(true);
            SetGridBackgroundTheme(e);
        }
               
        private void SetGridBackgroundTheme(TerminalTheme terminalTheme)
        {
            var color = terminalTheme.Colors.Background;
            var imageFile = terminalTheme.BackgroundImage;

            Brush backgroundBrush;

            if (imageFile != null && System.IO.File.Exists(imageFile.Path))
            {
                backgroundBrush = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(
                        imageFile.Path,
                        UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
            else
            {
                backgroundBrush = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    FallbackColor = color.FromString(),
                    TintColor = color.FromString(),
                    TintOpacity = ViewModel.BackgroundOpacity
                };
            }

            TerminalContainer.Background = backgroundBrush;
        }
    }
}