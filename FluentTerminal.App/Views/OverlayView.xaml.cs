using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using FluentTerminal.App.Converters;
using FluentTerminal.App.ViewModels;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class OverlayView : UserControl
    {
        private static readonly TrueToVisibleConverter VisibilityConverter = new TrueToVisibleConverter();

        public OverlayView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            // I've had to implement binding in code because when implemented in XAML it causes a binding error, probably when DataContext is null.
            if (args.NewValue is OverlayViewModel viewModel)
            {
                SetBinding(VisibilityProperty, new Binding
                {
                    Source = viewModel, 
                    Path = new PropertyPath(nameof(OverlayViewModel.ShowOverlay)), 
                    Converter = VisibilityConverter, 
                    Mode = BindingMode.OneWay
                });

                OverlayText.SetBinding(TextBlock.TextProperty, new Binding
                {
                    Source = viewModel, 
                    Path = new PropertyPath(nameof(OverlayViewModel.OverlayContent)), 
                    Mode = BindingMode.OneWay
                });
            }
        }
    }
}
