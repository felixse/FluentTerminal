using Windows.UI.Xaml.Controls;
using FluentTerminal.App.ViewModels;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class OverlayView : UserControl
    {
        public OverlayView(OverlayViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
