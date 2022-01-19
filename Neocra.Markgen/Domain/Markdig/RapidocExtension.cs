using Markdig;
using Markdig.Renderers;

namespace Neocra.Markgen.Domain.Markdig;

public class RapidocExtension : IMarkdownExtension
{
    private readonly CodeBlockRenderer codeBlockRenderer;

    public RapidocExtension(CodeBlockRenderer codeBlockRenderer)
    {
        this.codeBlockRenderer = codeBlockRenderer;
    }
    
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        // No setup is required when analyse markdown for RapidocExtension
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (!renderer.ObjectRenderers.Contains<CodeBlockRenderer>())
        {
            renderer.ObjectRenderers.ReplaceOrAdd<global::Markdig.Renderers.Html.CodeBlockRenderer>(this.codeBlockRenderer);
        }
    }
}