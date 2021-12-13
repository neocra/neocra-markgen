using System.Collections.Generic;

namespace Neocra.Markgen.Domain;

public class MenuItem
{
    public string Title { get; }
    public string? FilePath { get; }
    public string? UriPath { get;  }

    public List<MenuItem> Children { get; set; } = new List<MenuItem>();

    public MenuItem(string title, string? filePath, string? uriPath)
    {
        this.Title = title;
        this.FilePath = filePath;
        this.UriPath = uriPath;
    }
}