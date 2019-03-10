using CommandLine;

namespace FluentTerminal.App.CommandLineArguments
{
    [Verb("new")]
    public class NewVerb
    {
        [Value(0, Default = "")]
        public string Path { get; set; }

        [Option("command")]
        public string Command { get; set; }
    }
}
