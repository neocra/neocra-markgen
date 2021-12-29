using System.Linq;
using System.Threading.Tasks;
using Neocra.Markgen.Domain;
using NSubstitute;
using Scriban;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.Markgen.Tests;

public class ExtraFilesTests : BaseTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public ExtraFilesTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Should_copy_default_css_file_When_build_directory()
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("test.md", "/test.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.FileWriter.Received(1)
            .WriteAllTextAsync(".markgen/resources/default.css", Arg.Any<string>());
    }
    
    [Fact]
    public async Task Should_copy_custom_css_file_When_build_directory()
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", 
                GetDirectoryInfo("resources", "/resources"),
                GetFileInfo("my.css", "/resources/my.css"),
                GetFileInfo("test.md", "/test.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        this.FileWriter.Received(1)
            .Copy("/resources/my.css", ".markgen/resources/my.css", true);
    }
    
    [Fact]
    public async Task Should_header_section_contains_default_css_When_build_directory()
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", 
                GetDirectoryInfo("resources", "/resources"),
                GetFileInfo("my.css", "/resources/my.css"),
                GetFileInfo("test.md", "/test.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(),
                Arg.Is<TemplateContext>(t =>
                    t.Get<HeaderLink[]>("header").Any(h=>h.Rel == "stylesheet" && h.Href == "resources/default.css")));
    }
    
    [Fact]
    public async Task Should_add_css_file_to_header_section_When_build_directory()
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", 
                GetDirectoryInfo("resources", "/resources"),
                GetFileInfo("my.css", "/resources/my.css"),
                GetFileInfo("test.md", "/test.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(),
                Arg.Is<TemplateContext>(t =>
                    t.Get<HeaderLink[]>("header").Any(h=>h.Rel == "stylesheet" && h.Href == "resources/my.css")));
    }
}