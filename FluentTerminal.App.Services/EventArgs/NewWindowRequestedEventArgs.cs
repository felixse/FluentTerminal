namespace FluentTerminal.App.Services.EventArgs
{
    public class NewWindowRequestedEventArgs : System.EventArgs
    {
        public Models.Enums.ProfileSelection ShowSelection { get; set; }
    }
}