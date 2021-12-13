using System.CommandLine;
using Neocra.Markgen.Tools;

namespace Neocra.Markgen.Verbs.Watch
{
    public class WatchDefinition : ICommandDefinition
    {
        public Command Command(HandlerFactory handlerFactory)=>
            handlerFactory.AddHandler<WatchCommand>(
                new Command("watch")
                {
                    new Option<string>("--source"),
                });
    }
}