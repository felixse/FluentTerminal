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
}
