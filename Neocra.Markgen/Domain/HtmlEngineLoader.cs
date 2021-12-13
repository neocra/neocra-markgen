using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Neocra.Markgen.Domain;

public class HtmlEngineLoader : ITemplateLoader
{
    private readonly ILogger<HtmlEngineLoader> logger;

    public HtmlEngineLoader(ILogger<HtmlEngineLoader> logger)
    {
        this.logger = logger;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return $"Neocra.Markgen.Template.{templateName}.html.liquid";
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        throw new NotSupportedException();
    }

    public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(templatePath))
        {
            if (stream == null)
            {
                throw new FileNotFoundException(templatePath);
            }
                
            using (var reader = new StreamReader(stream))
            {
                var result = await reader.ReadToEndAsync();

                return result;
            }
        }
    }
}