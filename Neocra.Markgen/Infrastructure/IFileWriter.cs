using System.Threading.Tasks;

namespace Neocra.Markgen.Infrastructure;

public interface IFileWriter
{
    Task WriteAllTextAsync(string destinationFile, string content);
    void CreateDirectory(string directoryName);
}