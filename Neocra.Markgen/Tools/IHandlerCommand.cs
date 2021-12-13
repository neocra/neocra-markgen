using System.Threading.Tasks;

namespace Neocra.Markgen.Tools
{
    public interface IHandlerCommand<TOptions>
    {
        Task RunAsync(TOptions options);
    }
}