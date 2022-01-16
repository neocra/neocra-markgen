using System.IO;
using System.Runtime.CompilerServices;
using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;
using Xunit.Abstractions;

namespace Neocra.Markgen.Tests;

public class XuniTestConsole : IAnsiConsole
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly IAnsiConsole console;

    public XuniTestConsole(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        console = new AnsiConsoleFactory().Create(new AnsiConsoleSettings()
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.TrueColor,
            // Out = (IAnsiConsoleOutput) new AnsiConsoleOutput(this),
            Interactive = InteractionSupport.No,
            // ExclusivityMode = new NoopExclusivityMode(),
            Enrichment = new ProfileEnrichment()
            {
                UseDefaultEnrichers = false
            }
        });
    }

    public void Clear(bool home)
    {
        
    }

    public void Write(IRenderable renderable)
    {
        foreach (var segment in renderable.Render(new TestCapabilities(){}.CreateRenderContext(), 80))
        {
            this.testOutputHelper.WriteLine(segment.Text);
        }
    }

    public Profile Profile => console.Profile;
    public IAnsiConsoleCursor Cursor => console.Cursor;
    public IAnsiConsoleInput Input => console.Input;
    public IExclusivityMode ExclusivityMode => console.ExclusivityMode;
    public RenderPipeline Pipeline => console.Pipeline;
}