using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;
using FluentTerminal.App.Utilities;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalColorPicker : UserControl
    {
        private bool _editing;

        public static readonly DependencyProperty ColorNameProperty =
            DependencyProperty.Register(nameof(ColorName), typeof(string), typeof(TerminalColorPicker), new PropertyMetadata(null));

        public static readonly DependencyProperty EnableEditingProperty =
            DependencyProperty.Register(nameof(EnableEditing), typeof(bool), typeof(TerminalColorPicker), new PropertyMetadata(false));

        public static readonly DependencyProperty IsAlphaEnabledProperty =
                    DependencyProperty.Register(nameof(IsAlphaEnabled), typeof(bool), typeof(TerminalColorPicker), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedColorProperty =
                    DependencyProperty.Register(nameof(SelectedColor), typeof(string), typeof(TerminalColorPicker), new PropertyMetadata(null, (s, e) =>
                    {
                        if (s is TerminalColorPicker terminalColorPicker)
                        {
                            if (!terminalColorPicker._editing && e.NewValue != null)
                            {
                                var color = ((string)e.NewValue).FromString();
                                terminalColorPicker.colorPicker.Color = color;
                            }
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

        public string SelectedColor
        {
            get { return (string)GetValue(SelectedColorProperty); }
            set
            {
                SetValue(SelectedColorProperty, value);
                SetValue(ButtonBackgroundProperty, value);
            }
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
                    return new SolidColorBrush(((string)GetValue(SelectedColorProperty)).FromString());
                }
            }
        }

        private void ColorPicker_ColorChanged(MUXC.ColorPicker sender, MUXC.ColorChangedEventArgs args)
        {
            _editing = true;
            SelectedColor = args.NewColor.ToColorString(IsAlphaEnabled);
            _editing = false;
        }
    }
}