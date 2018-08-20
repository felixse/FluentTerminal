using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalColorPicker : UserControl
    {
        bool colorIsNull = false;

        public static readonly DependencyProperty ColorNameProperty =
            DependencyProperty.Register(nameof(ColorName), typeof(string), typeof(TerminalColorPicker), new PropertyMetadata(null));

        public static readonly DependencyProperty EnableEditingProperty =
            DependencyProperty.Register(nameof(EnableEditing), typeof(bool), typeof(TerminalColorPicker), new PropertyMetadata(false));

        public static readonly DependencyProperty IsAlphaEnabledProperty =
                    DependencyProperty.Register(nameof(IsAlphaEnabled), typeof(bool), typeof(TerminalColorPicker), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedColorProperty =
                    DependencyProperty.Register(nameof(SelectedColor), typeof(Color?), typeof(TerminalColorPicker), new PropertyMetadata(Colors.Transparent, (s, e) =>
                    {
                        if (s is TerminalColorPicker terminalColorPicker)
                        {
                            // Default color for the picker, when the value isn't yet set, is grey.
                            terminalColorPicker.colorIsNull = ((Color?)e.NewValue) == null;
                            terminalColorPicker.colorPicker.Color = (Color?)e.NewValue ?? Colors.Gray;
                        }
                    }));

        public static readonly DependencyProperty ButtonBackgroundProperty =
                    DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(TerminalColorPicker), new PropertyMetadata(false));


        public TerminalColorPicker()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public string ColorName
        {
            get { return (string)GetValue(ColorNameProperty); }
            set { SetValue(ColorNameProperty, value); }
        }

        public bool EnableEditing
        {
            get { return (bool)GetValue(EnableEditingProperty); }
            set { SetValue(EnableEditingProperty, value); }
        }

        public bool IsAlphaEnabled
        {
            get { return (bool)GetValue(IsAlphaEnabledProperty); }
            set { SetValue(IsAlphaEnabledProperty, value); }
        }

        public Color? SelectedColor
        {
            get { return (Color?)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public Brush ButtonBackground
        {
            get
            {
                if (SelectedColor == null)
                {
                    return (Brush)Application.Current.Resources["MissingColorSelectionBrush"];
                }
                else
                {
                    return new SolidColorBrush((Color)GetValue(SelectedColorProperty));
                }
            }
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            // When we set the color picker color, only update the selected color if the
            // color was set because it wasn't null.
            if (!colorIsNull)
            {
                SelectedColor = args.NewColor;
            }
        }
    }
}