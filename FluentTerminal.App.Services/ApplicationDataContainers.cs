namespace FluentTerminal.App.Services
{
    public class ApplicationDataContainers
    {
        public IApplicationDataContainer LocalSettings { get; set; }
        public IApplicationDataContainer RoamingSettings { get; set; }
        public IApplicationDataContainer Themes { get; set; }
        public IApplicationDataContainer KeyBindings { get; set; }
        public IApplicationDataContainer ShellProfiles { get; set; }
        public IApplicationDataContainer SshProfiles { get; set; }
    }
}