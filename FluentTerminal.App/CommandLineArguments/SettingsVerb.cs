using CommandLine;

namespace FluentTerminal.App.CommandLineArguments
{
    [Verb("settings")]
    public class SettingsVerb
    {
        [Option('i', "import",
                HelpText = "Import settings.")]
        public bool Import { get; set; }

        [Option('e', "export",
                HelpText = "Export settings.")]
        public bool Export { get; set; }
    }
}
