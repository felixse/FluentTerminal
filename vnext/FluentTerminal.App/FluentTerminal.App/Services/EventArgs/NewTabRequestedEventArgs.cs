namespace FluentTerminal.App.Services.EventArgs
{
    public class NewTabRequestedEventArgs : System.EventArgs
    {
        public string DragEventArgs { get; set; } // todo custom enum
        public int Position { get; set; }
    }
}
