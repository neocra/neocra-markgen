using System.Linq;
using System.Threading.Tasks;
using Neocra.Markgen.Domain;
using NSubstitute;
using Scriban;
using Scriban.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.Markgen.Tests;

public class MenuTests : BaseTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public MenuTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task Should_readme_file_is_convert_to_index_html_in_menu_When_build_directory()
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("README.md", "/README.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), 
                Arg.Is<TemplateContext>(t=> 
                    t.Get<MenuItem>("menu_item")
                    .Children.Any(c=>c.UriPath == "/index.html")));
    }
    
    [Theory]
    [InlineData(".git", "/.git", "Git")]
    [InlineData(".markgen", "/.markgen", "Markgen")]
    public async Task Should_technical_directory_is_not_in_menu_When_build_directory(string name, string directoryPath, string expectedIgnoreElement)
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetDirectoryInfo(name, directoryPath), GetFileInfo("Toto.md", "/Toto.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Is<TemplateContext>(t=>
                t.Get<MenuItem>("menu_item").Children.All(c => c.Title != expectedIgnoreElement)));
    }
    
    [Fact]
    public async Task Should_get_current_menu_When_build_directory()
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("Toto.md", "/Toto.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Is<TemplateContext>(t=> 
                t.Get<MarkdownPage>("model")
                .MenuItem.Title == "Toto"));
    }
}