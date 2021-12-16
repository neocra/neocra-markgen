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
            AddGetDirectoryContents(p, "", GetFileInfo("README.md", "/README.md"));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.FileWriter.Received(1)
            .WriteAllTextAsync(".markgen/resources/default.css", Arg.Any<string>());
    }
}