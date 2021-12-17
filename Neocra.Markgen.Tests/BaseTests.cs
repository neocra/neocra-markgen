using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Neocra.Core.Logger.Xunit;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Domain.Markdig;
using Neocra.Markgen.Infrastructure;
using Neocra.Markgen.Verbs.Build;
using Neocra.Markgen.Verbs.Watch;
using NSubstitute;
using Xunit.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Neocra.Markgen.Tests;

public class BaseTests
{
    protected readonly ServiceCollection Services = new();
    protected readonly IScriban Scriban;
    protected readonly IFileWriter FileWriter;

    protected BaseTests(ITestOutputHelper testOutputHelper)
    {
        this.Scriban = this.Services.AddSubstituteSingleton<IScriban>();
        this.FileWriter = this.Services.AddSubstituteSingleton<IFileWriter>();

        this.Services.AddSingleton(new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build());
        this.Services.AddSingleton<BuildCommand>();

        this.Services.AddSingleton<MarkdownTransform>();
        this.Services.AddSingleton<WatchCommand>();
        this.Services.AddSingleton<HtmlEngine>();
        this.Services.AddSingleton<HtmlEngineLoader>();
        this.Services.AddSingleton<CodeBlockRenderer>();
        this.Services.AddSingleton<RapidocExtension>();
        this.Services.AddSingleton<RendersProvider>();
        this.Services.AddSingleton<UriHelper>();

        this.Services.AddLogging(c => c.AddXUnitLogger(testOutputHelper));

    }

    private static IDirectoryContents GetDirectoryContents(params IFileInfo[] files)
    {
        var directoryContents = Substitute.For<IDirectoryContents>();
        directoryContents.GetEnumerator().Returns(
            files.ToArray().AsEnumerable().GetEnumerator());
        return directoryContents;
    }

    protected static IFileInfo GetDirectoryInfo(string directoryName, string physicalPath)
    {
        var directoryInfo = Substitute.For<IFileInfo>();
        directoryInfo.Name.Returns(directoryName);
        directoryInfo.PhysicalPath.Returns(physicalPath);
        directoryInfo.IsDirectory.Returns(true);
        return directoryInfo;
    }

    protected static IFileInfo GetFileInfo(string fileName, string filePath, string content = "# Head1\n## Head2")
    {
        var fileInfo = Substitute.For<IFileInfo>();
        fileInfo.Name.Returns(fileName);
        fileInfo.PhysicalPath.Returns(filePath);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        fileInfo.CreateReadStream().Returns(memoryStream);
        return fileInfo;
    }

    protected void AddFileProviderFactory(Action<IFileProvider> addProviders)
    {
        var fileProviderFactory = Substitute.For<IFileProviderFactory>();
        var fileProvider = Substitute.For<IFileProvider>();

        addProviders(fileProvider);

        fileProviderFactory.GetProvider("/")
            .Returns(fileProvider);

        this.Services.AddSingleton(fileProviderFactory);
    }

    protected static void AddGetDirectoryContents(IFileProvider fileProvider,
        string subpath,
        params IFileInfo[] files)
    {
        var directoryContents = GetDirectoryContents(files);
        fileProvider.GetDirectoryContents(subpath)
            .Returns(directoryContents);
    }
}