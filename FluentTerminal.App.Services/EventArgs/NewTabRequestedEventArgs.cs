using System;
using Windows.UI.Xaml;

namespace FluentTerminal.App.Services.EventArgs
{
    public class NewTabRequestedEventArgs : System.EventArgs
    {
        public DragEventArgs DragEventArgs { get; set; }
        public int Position { get; set; }
    }
}
