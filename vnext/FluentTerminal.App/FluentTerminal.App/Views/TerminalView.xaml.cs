using FluentTerminal.App.ViewModels;
using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentTerminal.Models.Messages;
using FluentTerminal.Models;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Input;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class TerminalView : UserControl,
        IRecipient<KeyBindingsChangedMessage>,
        IRecipient<TerminalOptionsChangedMessage>
    {
        private ITerminalView _terminalView;

        public TerminalView(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.SearchStarted += OnSearchStarted;
            ViewModel.Activated += OnActivated;
            ViewModel.ThemeChanged += OnThemeChanged;
            ViewModel.FindNextRequested += OnFindNextRequested;
            ViewModel.FindPreviousRequested += OnFindPreviousRequested;
            ViewModel.FontSizeChanged += OnFontSizeChanged;
            InitializeComponent();
            _terminalView = new XtermTerminalView();
            TerminalContainer.Children.Add((UIElement)_terminalView);
            _terminalView.InitializeAsync(ViewModel);
            ViewModel.TerminalView = _terminalView;
            ViewModel.Initialized = true;

            SetGridBackgroundTheme(ViewModel.TerminalTheme);

            viewModel.Overlay = (OverlayViewModel)MessageOverlay.DataContext;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void DisposalPrepare()
        {
            //Bindings.StopTracking();
            TerminalContainer.Children.Remove((UIElement)_terminalView);
            _terminalView?.Dispose();
            _terminalView = null;

            // Messenger.Default.Unregister(this); // todo do we need this?

            ViewModel.SearchStarted -= OnSearchStarted;
            ViewModel.Activated -= OnActivated;
            ViewModel.ThemeChanged -= OnThemeChanged;
            ViewModel.FindNextRequested -= OnFindNextRequested;
            ViewModel.FindPreviousRequested -= OnFindPreviousRequested;
            ViewModel.FontSizeChanged -= OnFontSizeChanged;

            ViewModel = null;
        }

        public TerminalViewModel ViewModel { get; private set; }

        private void OnActivated(object sender, EventArgs e)
        {
            _terminalView?.FocusTerminalAsync();
        }

        private void OnFindNextRequested(object sender, SearchRequest e)
        {
            _terminalView.FindNextAsync(e);
        }

        private void OnFindPreviousRequested(object sender, SearchRequest e)
        {
            _terminalView.FindPreviousAsync(e);
        }

        public void Receive(KeyBindingsChangedMessage message)
        {
            _terminalView.ChangeKeyBindingsAsync();
        }

        public void Receive(TerminalOptionsChangedMessage message)
        {
            _terminalView.ChangeOptionsAsync(message.TerminalOptions);
        }

        private void OnSearchStarted(object sender, EventArgs e)
        {
            SearchTextBox.Focus(FocusState.Programmatic);
            SearchTextBox.SelectAll();
        }

        private void OnSearchTextBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.FindPreviousCommand.Execute(null);
            }
        }

        private async void OnThemeChanged(object sender, TerminalTheme e)
        {
            await _terminalView.ChangeThemeAsync(e);
            SetGridBackgroundTheme(e);
        }

        private async void OnFontSizeChanged(object sender, int e)
        {
            await _terminalView.ChangeFontSize(e);
        }

        private void SetGridBackgroundTheme(TerminalTheme terminalTheme)
        {
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
                backgroundBrush = new SolidColorBrush(Colors.Transparent);
            }

            TerminalContainer.Background = backgroundBrush;
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.SearchHasFocus = true;
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.SearchHasFocus = false;
        }
    }
}