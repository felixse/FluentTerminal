namespace FluentTerminal.Models
{
    /// <summary>
    /// based on: http://www.0to255.com/E81123 using: 0, -2, -4, -6, -8, -10
    /// </summary>
    public class TabTheme
    {
        public string Background { get; set; } = "#4a050b";
        public string BackgroundPointerOver { get; set; } = "#690810";
        public string BackgroundPressed { get; set; } = "#890a15";
        public string BackgroundSelected { get; set; } = "#a90c19";
        public string BackgroundSelectedPointerOver { get; set; } = "#c80f1e";
        public string BackgroundSelectedPressed { get; set; } // = "#e81123"; testing the fallback converter
        public string Underline { get; set; } = "#e81123";
    }
}
