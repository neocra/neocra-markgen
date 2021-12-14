using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Domain.Markdig;

namespace Neocra.Markgen.Verbs.Build;

public class RendersProvider
{
    private readonly ILogger<RendersProvider> logger;
    private readonly MarkdownTransform markdownTransform;
    private readonly RapidocExtension rapidocExtension;

    public RendersProvider(ILogger<RendersProvider> logger, MarkdownTransform markdownTransform, RapidocExtension rapidocExtension)
    {
        this.logger = logger;
        this.markdownTransform = markdownTransform;
        this.rapidocExtension = rapidocExtension;
    }

    public async Task Renders(List<Entry> sourceEntries, MenuItem menu, string optionsSource, string destination, string baseUri)
    {
        foreach (var entry in sourceEntries)
        {
            this.logger.LogInformation("Render : {name}", entry.Name);
            switch (entry)
            {
                case MarkdownPage markdownPage:
                    await this.Render(menu, markdownPage, optionsSource, destination, baseUri);
                    break;
                case Image image:
                    await this.Render(image, optionsSource, destination);
                    break;
            }
        }
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
    
    private async Task Render(MenuItem menu, MarkdownPage markdownPage, string optionsSource, string destination, string baseUri)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Use(rapidocExtension)
            .Use<DiagramExtension>()
            .UseYamlFrontMatter()
            .Build();
        
        var content = DocumentProcessed(markdownPage.MarkdownDocument,
            link => $"{baseUri}{link.Url}")
            .ToHtml(pipeline);
        var markdownPage1 = new RenderModelMarkdownPage(menu, markdownPage, content);

        var mdFileInfo = markdownPage1.Model.FileInfo;
        var destinationFile = Path.GetRelativePath(optionsSource, mdFileInfo.PhysicalPath);
        destinationFile = Path.Combine(destination, destinationFile);
        destinationFile = Path.ChangeExtension(destinationFile, ".html");
            
        var directoryName = new FileInfo(destinationFile).DirectoryName;
        if (!string.IsNullOrEmpty(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        await File.WriteAllTextAsync(destinationFile, 
            await this.markdownTransform.RenderHtml(markdownPage1, baseUri));
    }

    private async Task Render(Image image, string source, string destination)
    {
        var destinationFile = Path.GetRelativePath(source, image.FileInfo.PhysicalPath);
        destinationFile = Path.Combine(destination, destinationFile);

        var directoryInfo = new FileInfo(destinationFile).Directory;
        if (directoryInfo is { Exists: false })
        {
            directoryInfo.Create();
        }
        
        File.Copy(image.FileInfo.PhysicalPath, destinationFile, true);
    }
}