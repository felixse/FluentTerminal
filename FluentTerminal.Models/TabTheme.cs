using System.ComponentModel;

namespace FluentTerminal.Models
{
    public class TabTheme : INotifyPropertyChanged
    {
        // warning disabled as the PropertyChanged is actually unnecessary here but caused a binding warning
#pragma warning disable CS0067 // The event 'TabTheme.PropertyChanged' is never used FluentTerminal.Models
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'TabTheme.PropertyChanged' is never used FluentTerminal.Models

        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public double BackgroundOpacity { get; set; } = 0.35;
        public double BackgroundPointerOverOpacity { get; set; } = 0.4;
        public double BackgroundPressedOpacity { get; set; } = 0.45;
        public double BackgroundSelectedOpacity { get; set; } = 0.7;
        public double BackgroundSelectedPointerOverOpacity { get; set; } = 0.8;
        public double BackgroundSelectedPressedOpacity { get; set; } = 0.9;
    }
}
