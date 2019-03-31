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
using System.Windows.Input;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.Views
{
    public sealed partial class OverlayView : UserControl
    {
        public OverlayView()
        {
            InitializeComponent();
        }
    }

    /**
    public sealed partial class OverlayView : UserControl
    {
        private readonly IOverlayView _overlayView;

        public OverlayView(OverlayViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            _overlayView.Initialize(ViewModel);
            DataContext = viewModel;
        }

        public OverlayViewModel ViewModel { get; }
    }**/


    /**public sealed partial class OverlayView : UserControl, INotifyPropertyChanged
    {
        public RelayCommand<string> UpdateOverlay { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public OverlayView()
        {
            InitializeComponent();
            UpdateOverlay = new RelayCommand<string>(UpdateOverlayText);
            DataContext = this;
        }

        // Not triggered
        private void UpdateOverlayText(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OverlayViewModel ViewModel { get; }
    }**/
}
