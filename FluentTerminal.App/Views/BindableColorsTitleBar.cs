using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;

namespace FluentTerminal.App.Views
{
    public sealed class BindableColorsTitleBar : Control
    {
        private readonly ApplicationViewTitleBar _titleBar;
        private readonly UISettings _uiSettings;
        private readonly CoreDispatcher _dispatcher;

        private bool _customValues;

        private Color? _buttonForeground;
        private Color? _buttonHoverBackground;
        private Color? _buttonPressedBackground;
        private Color? _buttonHoverForeground;
        private Color? _buttonPressedForeground;
        private Color? _inactiveForeground;

        public BindableColorsTitleBar()
        {
            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            _titleBar = ApplicationView.GetForCurrentView().TitleBar;

            _titleBar = ApplicationView.GetForCurrentView().TitleBar;
            _titleBar.ButtonBackgroundColor = Colors.Transparent;
            _titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            _titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];

            _dispatcher = Window.Current.Dispatcher;

            _buttonHoverBackground = _titleBar.ButtonHoverBackgroundColor = Color.FromArgb(24, 255, 255, 255);
            _buttonPressedBackground = _titleBar.ButtonPressedBackgroundColor = Color.FromArgb(48, 255, 255, 255);
        }

        private async void OnColorValuesChanged(UISettings sender, object args)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!_customValues)
                {
                    _titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];
                }
            });
        }

        public Color TitleBarForeground
        {
            get { return (Color)GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarForegroundProperty =
            DependencyProperty.Register(nameof(TitleBarForeground), typeof(Color), typeof(BindableColorsTitleBar), new PropertyMetadata(Colors.Transparent, TitleBarForegroundChanged));

        private static void TitleBarForegroundChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is BindableColorsTitleBar titleBarControl)
            {
                var color = (Color)args.NewValue;

                titleBarControl._buttonForeground = titleBarControl._titleBar.ButtonForegroundColor = color;
                titleBarControl._buttonPressedForeground = titleBarControl._titleBar.ButtonPressedForegroundColor = color;
                titleBarControl._buttonHoverForeground = titleBarControl._titleBar.ButtonHoverForegroundColor = color;

                titleBarControl._inactiveForeground = titleBarControl._titleBar.InactiveForegroundColor = Color.FromArgb(128, color.R, color.G, color.B);
            }
        }

        public void RestoreColors()
        {
            _titleBar.ButtonForegroundColor = _buttonForeground;
            _titleBar.ButtonHoverBackgroundColor = _buttonHoverBackground;
            _titleBar.ButtonPressedBackgroundColor = _buttonPressedBackground;
            _titleBar.ButtonPressedForegroundColor = _buttonPressedForeground;
            _titleBar.ButtonHoverForegroundColor = _buttonHoverForeground;
            _titleBar.InactiveForegroundColor = _inactiveForeground;

            _customValues = true;
        }

        public void ResetTitleBar()
        {
            _titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];
            _titleBar.ButtonHoverBackgroundColor = null;
            _titleBar.ButtonPressedBackgroundColor = null;
            _titleBar.ButtonPressedForegroundColor = null;
            _titleBar.ButtonHoverForegroundColor = null;
            _titleBar.InactiveForegroundColor = null;

            _customValues = false;
        }
    }
}
