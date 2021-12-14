using System.CommandLine;
using System.CommandLine.IO;
using Xunit.Abstractions;

namespace Neocra.Markgen.Tests;

public class XuniTestConsole : IConsole, IStandardStreamWriter
{
    private readonly ITestOutputHelper testOutputHelper;

    public XuniTestConsole(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        this.Out = this;
        this.Error = this;
    }

    public IStandardStreamWriter Out { get; }
    public bool IsOutputRedirected => false;
    public IStandardStreamWriter Error { get; }
    public bool IsErrorRedirected => false;
    public bool IsInputRedirected => false;
    
    public void Write(string value)
    {
        this.testOutputHelper.WriteLine(value);
    }
}