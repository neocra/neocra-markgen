using System.IO;
using System.Threading.Tasks;

namespace Neocra.Markgen.Infrastructure;

public class FileWriter : IFileWriter
{
    public Task WriteAllTextAsync(string destinationFile, string content)
    {
        return File.WriteAllTextAsync(destinationFile, content);
    }

    public void CreateDirectory(string directoryName)
    {
        Directory.CreateDirectory(directoryName);
    }

    public void Copy(string source, string destination, bool overwrite)
    {
        File.Copy(source, destination, true);
    }
}