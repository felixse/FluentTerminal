using FluentTerminal.App.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class EnvironmentVariablesView : UserControl
    {
        public ObservableCollection<EnvironmentVariableViewModel> EnvironmentVariables
        {
            get { return (ObservableCollection<EnvironmentVariableViewModel>)GetValue(EnvironmentVariablesProperty); }
            set { SetValue(EnvironmentVariablesProperty, value); }
        }

        public static readonly DependencyProperty EnvironmentVariablesProperty =
            DependencyProperty.Register(nameof(EnvironmentVariables), typeof(ObservableCollection<EnvironmentVariableViewModel>), typeof(EnvironmentVariablesView), new PropertyMetadata(null));

        public EnvironmentVariablesView()
        {
            InitializeComponent();
        }

        private void OnAddTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EnvironmentVariables.Add(new EnvironmentVariableViewModel());
        }

        private void OnRemoveTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is HyperlinkButton button)
            {
                var item = EnvironmentVariables.FirstOrDefault(x => x.Id == (Guid)button.Tag);
                EnvironmentVariables.Remove(item);
            }
        }
    }
}
