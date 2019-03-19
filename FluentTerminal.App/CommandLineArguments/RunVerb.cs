using CommandLine;

namespace FluentTerminal.App.CommandLineArguments
{
    [Verb("run")]
    public class RunVerb
    {
        [Value(0)]
        public string Command { get; set; }

        [Option("directory")]
        public string Directory { get; set; }

        [Option("theme")]
        public string Theme { get; set; }

        [Option("target")]
        public Target Target { get; set; }
    }
}
