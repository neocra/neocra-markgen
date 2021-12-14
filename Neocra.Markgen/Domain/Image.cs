using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Neocra.Markgen.Domain;

internal class Image : Entry
{
    public IFileInfo FileInfo { get; }

    public Image(IFileInfo fileInfo)
    {
        this.FileInfo = fileInfo;
    }

    public override string Name => this.FileInfo.PhysicalPath;
}