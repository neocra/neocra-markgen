using System.IO;
using Markdig.Syntax;

namespace Neocra.Markgen.Domain;

public class MarkdownPage : Entry
{
    public MarkdownPage(FileInfo fileInfo,
        MarkdownDocument markdownDocument,
        PageFrontMatter frontMatter)
    {
        this.FileInfo = fileInfo;
        this.MarkdownDocument = markdownDocument;
        this.FrontMatter = frontMatter;
    }

    public FileInfo FileInfo { get; }
    public MarkdownDocument MarkdownDocument { get; }

    public PageFrontMatter FrontMatter { get; }


    public override string Name => FileInfo.FullName;
}