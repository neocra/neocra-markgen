using Markdig;
using Markdig.Renderers;

namespace Neocra.Markgen.Domain.Markdig;

public class DiagramExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            var codeRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>()!;
            codeRenderer.BlocksAsDiv.Add("mermaid");
            codeRenderer.BlocksAsDiv.Add("nomnoml");
        }
    }
}