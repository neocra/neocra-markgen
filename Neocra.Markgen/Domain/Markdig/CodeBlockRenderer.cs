using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using YamlDotNet.Serialization;

namespace Neocra.Markgen.Domain.Markdig;

public class CodeBlockRenderer : global::Markdig.Renderers.Html.CodeBlockRenderer
{
    private readonly IDeserializer deserializer;

    public CodeBlockRenderer(IDeserializer deserializer)
    {
        this.deserializer = deserializer;
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        if (obj is FencedCodeBlock { Info: "rapidoc" })
        {
            renderer.EnsureLine();

            var content = obj.Lines.ToString();

            var rapidoc = this.deserializer.Deserialize<RapidocConfig>(content);

            renderer.Write("<div")
                .WriteAttributes(obj.TryGetAttributes())
                .Write('>');

            
            renderer.Write("<rapi-doc-mini ");

            if (!string.IsNullOrEmpty(rapidoc.SpecUrl))
            {
                renderer.Write($"spec-url=\"{rapidoc.SpecUrl}\" ");
            }

            if (!string.IsNullOrEmpty(rapidoc.MatchPaths))
            {
                renderer.Write($"match-paths=\"{rapidoc.MatchPaths}\" ");
            }

            if (!string.IsNullOrEmpty(rapidoc.PathsExpanded))
            {
                renderer.Write($"paths-expanded=\"{rapidoc.PathsExpanded}\" ");
            }

            if (!string.IsNullOrEmpty(rapidoc.MatchType))
            {
                renderer.Write($"match-type=\"{rapidoc.MatchType}\" ");
            }
            
            if (!string.IsNullOrEmpty(rapidoc.Theme))
            {
                renderer.Write($"theme=\"{rapidoc.Theme}\" ");
            }
            
            if (!string.IsNullOrEmpty(rapidoc.Layout))
            {
                renderer.Write($"layout=\"{rapidoc.Layout}\" ");
            }
            
            if (!string.IsNullOrEmpty(rapidoc.SchemaStyle))
            {
                renderer.Write($"schema-style=\"{rapidoc.SchemaStyle}\" ");
            }
            
            renderer.Write(">");
            renderer.Write("</rapi-doc-mini>");
            renderer.Write("</div>");
        }
        else
        {
            base.Write(renderer, obj);
        }
    }
}