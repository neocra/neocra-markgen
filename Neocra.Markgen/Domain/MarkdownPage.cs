using System.IO;
using Markdig.Syntax;
using Microsoft.Extensions.FileProviders;

namespace Neocra.Markgen.Domain;

public class MarkdownPage : Entry
{
    public MarkdownPage(IFileInfo fileInfo,
        MarkdownDocument markdownDocument,
        PageFrontMatter frontMatter)
    {
        this.FileInfo = fileInfo;
        this.MarkdownDocument = markdownDocument;
        this.FrontMatter = frontMatter;
    }

    public IFileInfo FileInfo { get; }
    public MarkdownDocument MarkdownDocument { get; }

    public PageFrontMatter FrontMatter { get; }


    public override string Name => FileInfo.PhysicalPath;
}