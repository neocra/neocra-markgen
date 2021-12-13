using System.CommandLine;
using Neocra.Markgen.Tools;

namespace Neocra.Markgen.Verbs.Build
{
    public class BuildDefinition : ICommandDefinition
    {
        public Command Command(HandlerFactory handlerFactory) =>
            handlerFactory.AddHandler<BuildCommand>(
                new Command("build")
                {
                    new Option<string>("--source"),
                    new Option<string>("--destination"),
                    new Option<string>("--base-uri"),
                });
    }

}