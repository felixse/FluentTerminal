using Microsoft.Xaml.Interactivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentTerminal.App.Actions
{
    public class FocusAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var control = TargetObject ?? sender as Control;
            if (control != null)
            {
                if (!control.IsLoaded)
                    control.Loaded += Control_Loaded;
                else
                    control.Focus(FocusState.Programmatic);
            }

            return null;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Control control = sender as Control;
            control.Focus(FocusState.Programmatic);
            control.Loaded -= Control_Loaded; // won't be needed anymore. Remove reference just in case
        }

        public Control TargetObject
        {
            get { return (Control)GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }

        public static readonly DependencyProperty TargetObjectProperty =
            DependencyProperty.Register(nameof(TargetObject), typeof(Control), typeof(FocusAction), new PropertyMetadata(null));
    }
}
