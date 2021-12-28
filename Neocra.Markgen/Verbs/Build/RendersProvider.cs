using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Domain.Markdig;
using Neocra.Markgen.Infrastructure;

namespace Neocra.Markgen.Verbs.Build;

public class RendersProvider
{
    private readonly ILogger<RendersProvider> logger;
    private readonly MarkdownTransform markdownTransform;
    private readonly RapidocExtension rapidocExtension;
    private readonly IFileWriter fileWriter;

    public RendersProvider(
        ILogger<RendersProvider> logger,
        MarkdownTransform markdownTransform,
        RapidocExtension rapidocExtension,
        IFileWriter fileWriter)
    {
        this.logger = logger;
        this.markdownTransform = markdownTransform;
        this.rapidocExtension = rapidocExtension;
        this.fileWriter = fileWriter;
    }

    public async Task Renders(List<Entry> sourceEntries, MenuItem menu, string optionsSource, string destination, string baseUri)
    {
        foreach (var entry in sourceEntries)
        {
            this.logger.LogInformation("Render : {name}", entry.Name);
            switch (entry)
            {
                case MarkdownPage markdownPage:
                    await this.Render(menu, markdownPage, optionsSource, destination, baseUri, 
                        GetHeader(sourceEntries, optionsSource, destination));
                    break;
                case Image image:
                    await this.Render(image, optionsSource, destination);
                    break;
                case CssFile cssFile:
                    await this.Render(cssFile, optionsSource, destination);
                    break;
            }
        }
    }

    private HeaderLink[] GetHeader(List<Entry> sourceEntries, string source, string destination)
    {
        return sourceEntries.OfType<CssFile>()
            .Where(c => 
                Path.GetRelativePath(source, c.FileInfo.PhysicalPath)
                .StartsWith("resources/"))
            .Select(c => new HeaderLink("stylesheet", Path.GetRelativePath(source, c.FileInfo.PhysicalPath)))
            .ToArray();
    }

    private MarkdownDocument DocumentProcessed(MarkdownDocument document, Func<LinkInline, string> urlRewriter)
    {
        var enumerator = document.Descendants().GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is LinkInline current)
                    current.Url = urlRewriter(current);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
                disposable.Dispose();
        }
        
        return document;
    }
    
    private async Task Render(MenuItem menu, MarkdownPage markdownPage, string optionsSource, string destination, string baseUri, HeaderLink[] header)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Use(this.rapidocExtension)
            .Use<DiagramExtension>()
            .UseYamlFrontMatter()
            .Build();
        
        var content = this.DocumentProcessed(markdownPage.MarkdownDocument,
            link => RewriteUri(baseUri, link))
            .ToHtml(pipeline);

        header =
            new[] { new HeaderLink("stylesheet", "resources/default.css") }
                .Union(header)
                .ToArray();
        
        foreach (var headerLink in header)
        {
            this.logger.LogDebug("Headerlink : {rel} {href}", headerLink.Rel, headerLink.Href);
        }
        
        var markdownPage1 = new RenderModelMarkdownPage(menu, markdownPage, content, baseUri, header);

        var mdFileInfo = markdownPage1.Model.FileInfo;
        var destinationFile = GetDestinationFile(optionsSource, destination, mdFileInfo);

        var directoryName = new FileInfo(destinationFile).DirectoryName;
        if (!string.IsNullOrEmpty(directoryName))
        {
            this.fileWriter.CreateDirectory(directoryName);
        }

        await this.fileWriter.WriteAllTextAsync(destinationFile, 
            await this.markdownTransform.RenderHtml(markdownPage1, baseUri));
    }

    private static string RewriteUri(string baseUri, LinkInline link)
    {
        if (link.Url == null)
        {
            return $"{baseUri}";
        }
        
        if (link.Url.EndsWith(".md"))
        {
            return $"{baseUri}{link.Url.Substring(0, link.Url.Length - 3)}.html";
        }

        return $"{baseUri}{link.Url}";
    }

    private static string GetDestinationFile(string optionsSource, string destination, IFileInfo mdFileInfo)
    {
        var physicalPath = mdFileInfo.PhysicalPath;
        if (mdFileInfo.Name == "README.md")
        {
            physicalPath = physicalPath.Substring(0, physicalPath.Length - 9) + "index";
        }

        var destinationFile = Path.GetRelativePath(optionsSource, physicalPath);
        destinationFile = Path.Combine(destination, destinationFile);
        destinationFile = Path.ChangeExtension(destinationFile, ".html");
        return destinationFile;
    }

    private async Task Render(ICopyFile copyFile, string source, string destination)
    {
        var destinationFile = Path.GetRelativePath(source, copyFile.FileInfo.PhysicalPath);
        destinationFile = Path.Combine(destination, destinationFile);

        var directoryInfo = new FileInfo(destinationFile).Directory;
        if (directoryInfo is { Exists: false })
        {
            directoryInfo.Create();
        }
        
        this.fileWriter.Copy(copyFile.FileInfo.PhysicalPath, destinationFile, true);
    }
}