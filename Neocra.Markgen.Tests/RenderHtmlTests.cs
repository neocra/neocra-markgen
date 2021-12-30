using System;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Neocra.Markgen.Domain;
using NSubstitute;
using Scriban;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.Markgen.Tests;

public class RenderHtmlTests : BaseTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public RenderHtmlTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }
    

    [Fact]
    public async Task Should_render_page_to_html_When_build_directory()
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("Toto.md", "/Toto.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Any<TemplateContext>());
    }

    [Fact]
    public async Task Should_readme_file_is_convert_to_index_html_When_build_directory()
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("README.md", "/README.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.FileWriter.Received(1)
            .Received(1)
            .WriteAllTextAsync(".markgen/index.html", Arg.Any<string>());
    }
    
    [Fact]
    public async Task Should_readme_file_is_convert_to_index_html_When_build_directory_and_sub_directory()
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetDirectoryInfo("subPath", "/subPath"));
            AddGetDirectoryContents(p, "subPath", GetFileInfo("README.md", "/subPath/README.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.FileWriter.Received(1)
            .Received(1)
            .WriteAllTextAsync(".markgen/subPath/index.html", Arg.Any<string>());
    }

    [Theory]
    [InlineData(true, "http://www.site.com","http://www.site.com")]
    [InlineData(true, "https://www.site.com","https://www.site.com")]
    [InlineData(true, "", "")]
    [InlineData(true, "/", "/base-uri/")]
    [InlineData(false, "", "")]
    [InlineData(false, "test.md", "test.html")]
    [InlineData(false, "test.html", "test.html")]
    [InlineData(false, "/test.html", "/test.html")]
    [InlineData(true, "test.md", "test.html")]
    [InlineData(true, "/test.md", "/base-uri/test.html")]
    [InlineData(true, "test.html", "test.html")]
    [InlineData(true, "/test.html", "/base-uri/test.html")]
    [InlineData(false, "README.md", "index.html")]
    [InlineData(false, "/README.md", "/index.html")]
    public async Task Should_apply_rewrite_rule_on_markdown_link_When_build_directory(bool baseUri, string uri, string expectedUri)
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("Toto.md", "/Toto.md", content:$"[MyLink]({uri})"));
        });

        if (baseUri)
        {
            await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/", "--base-uri", "/base-uri/");
        }
        else
        {
            await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");
        }

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(),
                ArgContainsAnyLinkIs(l => l.Url == expectedUri));
    }
    
    private static TemplateContext ArgContainsAnyLinkIs(Func<LinkInline, bool> predicate)
    {
        return Arg.Is<TemplateContext>(t =>
            t.Get<MarkdownPage>("model").MarkdownDocument
                .OfType<ParagraphBlock>()
                .Any(p => p.Inline != null && p.Inline
                    .OfType<LinkInline>().Any(predicate)));
    }
}