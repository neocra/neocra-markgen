using Spectre.Console.Cli;

namespace Neocra.Markgen.Verbs.Build
{
    public class BuildOptions : CommandSettings
    {
        [CommandOption("-s|--source")]
        public string? Source { get; set; }
        [CommandOption("-d|--destination")]
        public string? Destination { get; set; }
        [CommandOption("--base-uri")]
        public string? BaseUri { get; set; }
    }
}