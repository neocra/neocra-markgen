using System.IO;

namespace Neocra.Markgen.Domain;

public class UriHelper
{
    public string GetUri(string baseUri, string baseDirectory, string infoFullName)
    {
        var path = Path.GetRelativePath(baseDirectory, infoFullName);

        return GetLinkUrl(baseUri, $"/{path}");
    }
    
    public string GetLinkUrl(string baseUri, string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }
        
        if (url.StartsWith("http://")
            || url.StartsWith("https://"))
        {
            return url;
        }

        var b = GetBaseUri(baseUri, url);

        if (url.EndsWith("README.md"))
        {
            return $"{b}{url.Substring(0, url.Length - 9)}index.html";
        }
            
        if (url.EndsWith(".md"))
        {
            return $"{b}{url.Substring(0, url.Length - 3)}.html";
        }
            
        return $"{b}{url}";

    }

    private static string GetBaseUri(string baseUri, string url)
    {
        var b = string.IsNullOrEmpty(baseUri) ? "" : baseUri;

        if (!url.StartsWith("/"))
        {
            return string.Empty;
        }

        if (b.EndsWith("/"))
        {
            return b.Substring(0, b.Length - 1);
        }

        return b;
    }
}