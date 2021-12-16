using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Neocra.Markgen.Infrastructure;

public class PhysicalFileProviderFactory : IFileProviderFactory
{
    public IFileProvider GetProvider(string directorySourceFullName)
    {
        return new PhysicalFileProvider(directorySourceFullName, ExclusionFilters.None);
    }
}