using System.Linq;
using System.Threading.Tasks;
using Neocra.Markgen.Domain;
using NSubstitute;
using Scriban;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.Markgen.Tests;

public class RapidocRenderTests : BaseTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public RapidocRenderTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("```rapidoc\nspec-url: http://\n```\n", "<div class=\"language-rapidoc\"><rapi-doc-mini spec-url=\"http://\" ></rapi-doc-mini></div>")]
    [InlineData("```rapidoc {.to-right}\nspec-url: http://\n```\n", "<div class=\"to-right language-rapidoc\"><rapi-doc-mini spec-url=\"http://\" ></rapi-doc-mini></div>")]
    public async Task Should_convert_code_block_in_rapidoc_balise_When_build_directory_with_rapidoc(string markdown, string html)
    {
        this.AddFileProviderFactory(p =>
        {
            AddGetDirectoryContents(p, "", GetFileInfo("README.md", "/README.md", 
markdown));
        });

        await Program.RunAsync(this.Services, new XuniTestConsole(this.testOutputHelper), "build", "--source", "/");

        await this.Scriban.Received(1)
            .RenderAsync(Arg.Any<string>(),
                Arg.Is<TemplateContext>(t =>
                    t.Get<string>("markdown_page_content_html") == html));
    }
}