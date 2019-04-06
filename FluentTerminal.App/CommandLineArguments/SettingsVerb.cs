using CommandLine;

namespace FluentTerminal.App.CommandLineArguments
{
    [Verb("settings")]
    public class SettingsVerb
    {
        [Option("import")]
        public bool Import { get; set; }

        [Option("export")]
        public bool Export { get; set; }
    }
}
