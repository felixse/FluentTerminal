using System;

namespace FluentTerminal.App.Services.EventArgs
{
    public class NewWindowRequestedEventArgs : System.EventArgs
    {
        public Models.Enums.NewWindowAction Action { get; set; }
        public Guid ProfileId { get; set; }
    }
}