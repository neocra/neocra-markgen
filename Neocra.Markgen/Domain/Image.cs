using System.IO;

namespace Neocra.Markgen.Domain;

internal class Image : Entry
{
    public FileInfo FileInfo { get; }

    public Image(FileInfo fileInfo)
    {
        this.FileInfo = fileInfo;
    }

    public override string Name => this.FileInfo.FullName;
}