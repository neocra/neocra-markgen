using Spectre.Console.Cli;

namespace Neocra.Markgen.Verbs.Watch
{
    public class WatchOptions: CommandSettings
    {
        [CommandOption("-s|--source")]
        public string? Source { get; set; }
        
        [CommandOption("-d|--destination")]
        public string? Destination { get; set; }
    }
}