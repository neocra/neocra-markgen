using System.CommandLine;

namespace Neocra.Markgen.Tools
{
    public interface ICommandDefinition
    {
        Command Command(HandlerFactory handlerFactory);
    }
}