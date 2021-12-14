using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Verbs.Build;
using NSubstitute;
using Scriban;
using Scriban.Syntax;
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
        var fileProviderFactory = Substitute.For<IFileProviderFactory>();

        var fileProvider = Substitute.For<IFileProvider>();
        var directoryContents = GetDirectoryContents(
            GetFileInfo());
        fileProvider.GetDirectoryContents("")
            .Returns(directoryContents);
        
        fileProviderFactory.GetProvider(Arg.Any<string>())
            .Returns(fileProvider);
        this.Services.AddSingleton(fileProviderFactory);
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Any<TemplateContext>());
    }
    
    [Theory]
    [InlineData(".git", "/.git", "Git")]
    [InlineData(".markgen", "/.markgen", "Markgen")]
    public async Task Should_technical_directory_is_not_in_menu_When_build_directory(string name, string directoryPath, string expectedIgnoreElement)
    {
        var fileProviderFactory = Substitute.For<IFileProviderFactory>();

        var fileProvider = Substitute.For<IFileProvider>();
        var directoryContents = GetDirectoryContents(
            GetDirectoryInfo(name, directoryPath), 
            GetFileInfo());
        fileProvider.GetDirectoryContents("")
            .Returns(directoryContents);
        
        fileProviderFactory.GetProvider(Arg.Any<string>())
            .Returns(fileProvider);
        this.Services.AddSingleton(fileProviderFactory);
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Is<TemplateContext>(t=>Is(t, expectedIgnoreElement)));
    }

    private static IDirectoryContents GetDirectoryContents(params IFileInfo[] files)
    {
        var directoryContents = Substitute.For<IDirectoryContents>();
        directoryContents.GetEnumerator().Returns(
            files.ToArray().AsEnumerable().GetEnumerator());
        return directoryContents;
    }

    private static IFileInfo GetDirectoryInfo(string directoryName, string physicalPath)
    {
        var directoryInfo = Substitute.For<IFileInfo>();
        directoryInfo.Name.Returns(directoryName);
        directoryInfo.PhysicalPath.Returns(physicalPath);
        directoryInfo.IsDirectory.Returns(true);
        return directoryInfo;
    }

    private static IFileInfo GetFileInfo()
    {
        var fileInfo = Substitute.For<IFileInfo>();
        fileInfo.Name.Returns("Toto.md");
        fileInfo.PhysicalPath.Returns("/Toto.md");
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("# Toto"));
        fileInfo.CreateReadStream().Returns(memoryStream);
        return fileInfo;
    }

    private static bool Is(TemplateContext t, string title)
    {
        var value = t.GetValue(
            ScriptVariable.Create("menu_item", ScriptVariableScope.Global));
        return ((MenuItem)value).Children.All(c => c.Title != title);
    }
}