using Microsoft.Extensions.FileProviders;

namespace Neocra.Markgen.Domain;

public class CssFile : Entry, ICopyFile
{
    public CssFile(IFileInfo info)
    {
        this.FileInfo = info;
    }

    public override string Name => this.FileInfo.PhysicalPath;

    public IFileInfo FileInfo { get; }
}