using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FluentTerminal.App.Utilities
{
    class InteractiveSurface : ContentControl
    {
        public static readonly DependencyProperty HoveredProperty =
            DependencyProperty.Register(nameof(Hovered), typeof(bool), typeof(InteractiveSurface), new PropertyMetadata(false));

        public bool Hovered
        {
            get => (bool)GetValue(HoveredProperty);
            set => SetValue(HoveredProperty, value);
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            Hovered = true;
        }

        protected override void OnPointerCanceled(PointerRoutedEventArgs e)
        {
            base.OnPointerCanceled(e);
            Hovered = false;
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            Hovered = false;
        }
    }
}
