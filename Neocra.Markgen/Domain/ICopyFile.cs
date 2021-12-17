using Microsoft.Extensions.FileProviders;

namespace Neocra.Markgen.Domain;

internal interface ICopyFile
{
    IFileInfo FileInfo { get; }
}