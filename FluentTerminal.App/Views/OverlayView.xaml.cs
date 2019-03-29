using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views
{
    /*
    public sealed partial class OverlayView : UserControl
    {

        public int MyProperty
        {
            get { return (int)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

         Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(int), typeof(ownerclass), new PropertyMetadata(0));

        public OverlayView()
        {
           InitializeComponent();
            DataContext = this;
        }
    }*/

    public sealed partial class OverlayView : UserControl
    {
        private readonly IOverlayView _overlayView;

        public OverlayView(OverlayViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            _overlayView.Initialize(ViewModel);
        }

        public OverlayViewModel ViewModel { get; }

    }
}