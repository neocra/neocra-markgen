namespace Neocra.Markgen.Domain;

public class RenderModelMarkdownPage
{
    public RenderModelMarkdownPage(MenuItem menuItem, MarkdownPage model, string markdownPageContentHtml)
    {
        this.MenuItem = menuItem;
        this.Model = model;
        this.MarkdownPageContentHtml = markdownPageContentHtml;
    }

    public MenuItem MenuItem { get; }
    public MarkdownPage Model { get; }
    
    public string MarkdownPageContentHtml { get; }

}