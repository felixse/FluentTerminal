using System.Collections.Generic;

namespace FluentTerminal.Models
{
    public class TabTheme
    {
        public static IEnumerable<TabTheme> Themes = new[] {
            new TabTheme
            {
                Name = "Red",
                Color = "#e81123",
                BackgroundOpacity = 0.1,
                BackgroundPointerOverOpacity = 0.2,
                BackgroundPressedOpacity = 0.4,
                BackgroundSelectedOpacity = 0.6,
                BackgroundSelectedPointerOverOpacity = 0.7,
                BackgroundSelectedPressedOpacity = 0.9
            },
            new TabTheme
            {
                Name = "None",
                Color = "#e81123", // create fallback for underline
                BackgroundOpacity = double.NaN,
                BackgroundPointerOverOpacity = double.NaN,
                BackgroundPressedOpacity = double.NaN,
                BackgroundSelectedOpacity = double.NaN,
                BackgroundSelectedPointerOverOpacity = double.NaN,
                BackgroundSelectedPressedOpacity = double.NaN
            }
        };

        public string Name { get; set; }
        public string Color { get; set; }
        public double BackgroundOpacity { get; set; }
        public double BackgroundPointerOverOpacity { get; set; }
        public double BackgroundPressedOpacity { get; set; }
        public double BackgroundSelectedOpacity { get; set; }
        public double BackgroundSelectedPointerOverOpacity { get; set; }
        public double BackgroundSelectedPressedOpacity { get; set; }
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
