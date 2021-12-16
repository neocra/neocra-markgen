namespace Neocra.Markgen.Domain;

public class RenderModelMarkdownPage
{
    public RenderModelMarkdownPage(MenuItem menuItem, MarkdownPage model, string markdownPageContentHtml, string baseUri)
    {
        this.MenuItem = menuItem;
        this.Model = model;
        this.MarkdownPageContentHtml = markdownPageContentHtml;
        this.BaseUri = baseUri;
    }

    public MenuItem MenuItem { get; }
    public MarkdownPage Model { get; }
    
    public string MarkdownPageContentHtml { get; }
    public string BaseUri { get; }

}