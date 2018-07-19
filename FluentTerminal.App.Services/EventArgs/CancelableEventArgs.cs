namespace FluentTerminal.App.Services.EventArgs
{
    public class CancelableEventArgs : System.EventArgs
    {
        public bool Cancelled { get; set; }
    }
}