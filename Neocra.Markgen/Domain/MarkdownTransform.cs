using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Neocra.Markgen.Domain.Markdig;
using YamlDotNet.Serialization;

namespace Neocra.Markgen.Domain;

public class MarkdownTransform
{
    private readonly HtmlEngine engine;
    private readonly IDeserializer deserializer;
    private readonly RapidocExtension rapidocExtension;
    private readonly UriHelper uriHelper;

    public MarkdownTransform(HtmlEngine engine, 
        IDeserializer deserializer,
        RapidocExtension rapidocExtension,
        UriHelper uriHelper)
    {
        this.engine = engine;
        this.deserializer = deserializer;
        this.rapidocExtension = rapidocExtension;
        this.uriHelper = uriHelper;
    }

    public async Task<string> Transform(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .UseDiagrams()
            .Build();

        var document = Markdown.Parse(markdown, pipeline, new MarkdownParserContext());
        var modelMarkdownFile = new MarkdownPage(new PhysicalFileInfo(new FileInfo("")), document, this.GetFrontMatter(document), new MenuItem("", "",""));

        return await this.RenderHtml(new RenderModelMarkdownPage(new MenuItem("", "", ""), modelMarkdownFile, document.ToHtml(pipeline), "", Array.Empty<HeaderLink>()), string.Empty);
    }

    public async Task<string> RenderHtml(RenderModelMarkdownPage modelMarkdownPageMarkdownFile, string baseUri)
    {
        return await this.engine.CompileRenderAsync("View", modelMarkdownPageMarkdownFile);
    }

    public async Task<MarkdownPage> GetModelMarkdownFile(IFileInfo file, string baseUri, string baseDirectory)
    {
        using (var reader = new StreamReader(file.CreateReadStream()))
        {
            var sourceMd = await reader.ReadToEndAsync();
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Use(rapidocExtension)
                .Use<DiagramExtension>()
                .UseYamlFrontMatter()
                .Build();

            var document = Markdown.Parse(sourceMd, pipeline, new MarkdownParserContext());
            var pageFrontMatter = this.GetFrontMatter(document);
            var title = GetTitleFromMarkdownFile(pageFrontMatter, file);
            var menuItem = new MenuItem(title, file.PhysicalPath, this.uriHelper.GetUri(baseUri, baseDirectory, file.PhysicalPath));

            return new MarkdownPage(file, document, pageFrontMatter, menuItem);
        }
    }
    
    private string GetTitleFromMarkdownFile(PageFrontMatter modelMarkdownFile, IFileInfo file)
    {
        var s =modelMarkdownFile.Title;
        if (s != null)
        {
            return s;
        }

        return Path.GetFileNameWithoutExtension(file.Name)
            .Humanize();
    }
    
    private PageFrontMatter GetFrontMatter(MarkdownDocument document)
    {
        var yamlFrontMatterBlock = document
            .OfType<YamlFrontMatterBlock>()
            .FirstOrDefault();

        var yaml = yamlFrontMatterBlock?.Lines.ToString();

        if (string.IsNullOrEmpty(yaml))
        {
            return new PageFrontMatter();
        }
        
        return this.deserializer.Deserialize<PageFrontMatter>(yaml);
    }
}