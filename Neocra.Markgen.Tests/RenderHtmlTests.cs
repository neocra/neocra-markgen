using System;
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
    
    [Fact]
    public async Task Should_readme_file_is_convert_to_index_html_in_menu_When_build_directory()
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("README.md", "/README.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), 
                Arg.Is<TemplateContext>(t=>GetMenuItem(t).Children.Any(c=>c.UriPath == "/index.html")));
    }
    
    [Theory]
    [InlineData(".git", "/.git", "Git")]
    [InlineData(".markgen", "/.markgen", "Markgen")]
    public async Task Should_technical_directory_is_not_in_menu_When_build_directory(string name, string directoryPath, string expectedIgnoreElement)
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetDirectoryInfo(name, directoryPath), GetFileInfo("Toto.md", "/Toto.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Is<TemplateContext>(t=>Is(t, expectedIgnoreElement)));
    }
    
    [Fact]
    public async Task Should_get_current_menu_When_build_directory()
    {
        AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("Toto.md", "/Toto.md"));
        });
        
        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(), Arg.Is<TemplateContext>(t=> ((MarkdownPage)t.GetValue(
                ScriptVariable.Create("model", ScriptVariableScope.Global))).MenuItem.Title == "Toto"));
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

    private static IFileInfo GetFileInfo(string fileName, string filePath)
    {
        var fileInfo = Substitute.For<IFileInfo>();
        fileInfo.Name.Returns(fileName);
        fileInfo.PhysicalPath.Returns(filePath);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("# Toto"));
        fileInfo.CreateReadStream().Returns(memoryStream);
        return fileInfo;
    }

    private static bool Is(TemplateContext t, string title)
    {
        return GetMenuItem(t).Children.All(c => c.Title != title);
    }

    private static MenuItem GetMenuItem(TemplateContext t)
    {
        var value = t.GetValue(
            ScriptVariable.Create("menu_item", ScriptVariableScope.Global));
        var menuItem = ((MenuItem)value);
        return menuItem;
    }
    
    
    private void AddFileProviderFactory(Action<IFileProvider> addProviders)
    {
        var fileProviderFactory = Substitute.For<IFileProviderFactory>();
        var fileProvider = Substitute.For<IFileProvider>();

        addProviders(fileProvider);

        fileProviderFactory.GetProvider("/")
            .Returns(fileProvider);
        
        this.Services.AddSingleton(fileProviderFactory);
    }
    
    private static void AddGetDirectoryContents(IFileProvider fileProvider,
        string subpath,
        params IFileInfo[] files)
    {
        var directoryContents = GetDirectoryContents(files);
        fileProvider.GetDirectoryContents(subpath)
            .Returns(directoryContents);
    }
}