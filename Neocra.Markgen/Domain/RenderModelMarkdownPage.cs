namespace Neocra.Markgen.Domain;

public class RenderModelMarkdownPage
{
    public RenderModelMarkdownPage(
        MenuItem menuItem, 
        MarkdownPage model, 
        string markdownPageContentHtml,
        string baseUri, 
        HeaderLink[] header,
        bool hasRightSample)
    {
        this.MenuItem = menuItem;
        this.Model = model;
        this.MarkdownPageContentHtml = markdownPageContentHtml;
        this.BaseUri = baseUri;
        this.Header = header;
        this.HasRightSample = hasRightSample;
    }

    public MenuItem MenuItem { get; }
    public MarkdownPage Model { get; }

    public bool HasRightSample { get; }
    
    public string MarkdownPageContentHtml { get; }
    public string BaseUri { get; }
    
    public HeaderLink[] Header { get; }
}