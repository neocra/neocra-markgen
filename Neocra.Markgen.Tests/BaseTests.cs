using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neocra.Core.Logger.Xunit;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Domain.Markdig;
using Neocra.Markgen.Infrastructure;
using Neocra.Markgen.Verbs.Build;
using Neocra.Markgen.Verbs.Watch;
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace Neocra.Markgen.Tests;

public class BaseTests
{
    protected readonly ServiceCollection Services = new();
    protected readonly IScriban Scriban;

    protected BaseTests(ITestOutputHelper testOutputHelper)
    {
        this.Scriban = this.Services.AddSubstituteSingleton<IScriban>();
        this.Services.AddSubstituteSingleton<IDeserializer>();

        this.Services.AddSingleton<BuildCommand>();

        this.Services.AddSingleton<MarkdownTransform>();
        this.Services.AddSingleton<WatchCommand>();
        this.Services.AddSingleton<HtmlEngine>();
        this.Services.AddSingleton<HtmlEngineLoader>();
        this.Services.AddSingleton<CodeBlockRenderer>();
        this.Services.AddSingleton<RapidocExtension>();
        this.Services.AddSingleton<RendersProvider>();
        
        this.Services.AddLogging(c => c.AddXUnitLogger(testOutputHelper));

    }
}