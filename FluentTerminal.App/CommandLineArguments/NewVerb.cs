using CommandLine;

namespace FluentTerminal.App.CommandLineArguments
{
    [Verb("new")]
    public class NewVerb
    {
        [Value(0)]
        public string Directory { get; set; }

        [Option("profile")]
        public string Profile { get; set; }

        [Option("target")]
        public Target Target { get; set; }
    }
}
