namespace FluentTerminal.Models
{
    public class TabTheme
    {
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
