using Microsoft.Extensions.FileProviders;

namespace Neocra.Markgen.Infrastructure;

public interface IFileProviderFactory
{
    IFileProvider GetProvider(string directorySourceFullName);
}