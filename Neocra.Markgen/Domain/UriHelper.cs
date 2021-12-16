using System.IO;

namespace Neocra.Markgen.Domain;

public class UriHelper
{
    public string GetUri(string baseUri, string baseDirectory, string infoFullName)
    {
        if (infoFullName.EndsWith("README.md"))
        {
            infoFullName = infoFullName.Substring(0, infoFullName.Length - 9) + "index";
        }
        
        var path = Path.GetRelativePath(baseDirectory, infoFullName);

        var extension = Path.GetExtension(path);
        return $"{baseUri}/{path.Substring(0,path.Length - extension.Length)}.html";
    }
}