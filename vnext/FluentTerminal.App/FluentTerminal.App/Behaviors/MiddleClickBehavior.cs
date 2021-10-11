using Microsoft.Xaml.Interactivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace FluentTerminal.App.Behaviors
{
    public class MiddleClickBehavior : Trigger<UIElement>
    {
        private bool _middleButtonPressed;

        protected override void OnAttached()
        {
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerReleased += OnPointerReleased;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PointerPressed -= OnPointerPressed;
            AssociatedObject.PointerReleased -= OnPointerReleased;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(AssociatedObject);
            _middleButtonPressed = point.Properties.IsMiddleButtonPressed;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(AssociatedObject);
            var middleButtonStillPressed = point.Properties.IsMiddleButtonPressed;

            if (_middleButtonPressed && !middleButtonStillPressed)
            {
                Interaction.ExecuteActions(AssociatedObject, Actions, null);
            }
        }
    }
}