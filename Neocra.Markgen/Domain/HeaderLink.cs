namespace Neocra.Markgen.Domain;

public class HeaderLink
{
    public HeaderLink(string rel, string href)
    {
        this.Rel = rel;
        this.Href = href;
    }

    public string Rel { get; set; }
    public string Href { get; set; }
}