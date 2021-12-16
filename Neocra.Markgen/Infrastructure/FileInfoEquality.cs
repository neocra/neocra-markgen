using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Neocra.Markgen.Infrastructure;

public class FileInfoEquality : IEqualityComparer<IFileInfo>
{
    public bool Equals(IFileInfo? x, IFileInfo? y)
    {
        return XFullName(x) == XFullName(y);
    }
    
    private static string? XFullName(IFileInfo? x)
    {
        if (x?.IsDirectory ?? true)
        {
            return x?.PhysicalPath;
        }
        
        return x.PhysicalPath.Substring(0, x.PhysicalPath.Length - Path.GetExtension(x.PhysicalPath).Length);
    }
    
    public int GetHashCode(IFileInfo obj)
    {
        return 0;
    }
}