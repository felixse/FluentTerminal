using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed class BindableColorsTitleBar : Control
    {
        private ApplicationViewTitleBar _titleBar;

        private Color? _buttonForeground;
        private Color? _buttonHoverBackground;
        private Color? _buttonPressedBackground;
        private Color? _buttonHoverForeground;
        private Color? _buttonPressedForeground;
        private Color? _inactiveForeground;

        public BindableColorsTitleBar()
        {
            DefaultStyleKey = typeof(BindableColorsTitleBar);
            _titleBar = ApplicationView.GetForCurrentView().TitleBar;
            _buttonHoverBackground = _titleBar.ButtonHoverBackgroundColor = Color.FromArgb(24, 255, 255, 255);
            _buttonPressedBackground = _titleBar.ButtonPressedBackgroundColor = Color.FromArgb(48, 255, 255, 255);
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
        }

        public void ResetTitleBar()
        {
            _titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];
            _titleBar.ButtonHoverBackgroundColor = null;
            _titleBar.ButtonPressedBackgroundColor = null;
            _titleBar.ButtonPressedForegroundColor = null;
            _titleBar.ButtonHoverForegroundColor = null;
            _titleBar.InactiveForegroundColor = null;
        }
    }
}
