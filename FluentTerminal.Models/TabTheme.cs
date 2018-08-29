using System.Collections.Generic;

namespace FluentTerminal.Models
{
    public class TabTheme
    {
        public static IEnumerable<TabTheme> Themes = new[] {
            new TabTheme
            {
                Name = "None",
                BackgroundOpacity = double.NaN,
                BackgroundPointerOverOpacity = double.NaN,
                BackgroundPressedOpacity = double.NaN,
                BackgroundSelectedOpacity = double.NaN,
                BackgroundSelectedPointerOverOpacity = double.NaN,
                BackgroundSelectedPressedOpacity = double.NaN
            },
            new TabTheme
            {
                Name = "Red",
                Color = "#E81123"
            },
            new TabTheme
            {
                Name = "Green",
                Color = "#10893E"
            },
            new TabTheme
            {
                Name = "Blue",
                Color = "#0078D7"
            },
            new TabTheme
            {
                Name = "Purple",
                Color = "#881798"
            },
            new TabTheme
            {
                Name = "Orange",
                Color = "#FF8C00"
            },
            new TabTheme
            {
                Name = "Teal",
                Color = "#00B7C3"
            }
        };

        public string Name { get; set; }
        public string Color { get; set; }
        public double BackgroundOpacity { get; set; } = 0.35;
        public double BackgroundPointerOverOpacity { get; set; } = 0.4;
        public double BackgroundPressedOpacity { get; set; } = 0.45;
        public double BackgroundSelectedOpacity { get; set; } = 0.7;
        public double BackgroundSelectedPointerOverOpacity { get; set; } = 0.8;
        public double BackgroundSelectedPressedOpacity { get; set; } = 0.9;
    }

    public enum TabThemeKey
    {
        Background,
        BackgroundPointerOver,
        BackgroundPressed,
        BackgroundSelected,
        BackgroundSelectedPointerOver,
        BackgroundSelectedPressed
    }
}
