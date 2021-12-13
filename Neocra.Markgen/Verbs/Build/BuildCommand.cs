using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Tools;

namespace Neocra.Markgen.Verbs.Build;

public class BuildCommand : IHandlerCommand<BuildOptions>
{
    private readonly ILogger logger;
    private readonly RendersProvider rendersProvider;
    private readonly MarkdownTransform markdownTransform;

    public BuildCommand(MarkdownTransform markdownTransform, ILogger<BuildCommand> logger, RendersProvider rendersProvider)
    {
        this.markdownTransform = markdownTransform;
        this.logger = logger;
        this.rendersProvider = rendersProvider;
    }
        
    public async Task RunAsync(BuildOptions options)
    {
        this.logger.LogInformation("Begin build");
        var optionsSource = ".";
        if (!string.IsNullOrEmpty(options.Source))
        {
            optionsSource = options.Source;
        }

        var destination = "Markgen";
        if (!string.IsNullOrEmpty(options.Destination))
        {
            destination = options.Destination;
        }
            
        await CopyEmbeddedFile(destination, "splendor.min.css");
            
        var (sourceEntries, menu) = await this.GetSources(optionsSource, optionsSource, options.BaseUri ?? string.Empty);

        this.logger.LogDebug("Menu is :");

        this.LogMenu(menu, string.Empty);

        await this.rendersProvider.Renders(sourceEntries, menu, optionsSource, destination, options.BaseUri ?? string.Empty);
    }
    
    private void LogMenu(MenuItem menu, string baseString)
    {
        this.logger.LogDebug(baseString + " {menu} ({path})", menu.Title, menu.FilePath);
        foreach (var menuChild in menu.Children)
        {
            this.LogMenu(menuChild, baseString + ">");
        }
    }

    private async Task<(List<Entry>, MenuItem)> GetSources(string source, string baseDirectory, string baseUri)
    {
        var entries = new List<Entry>();
        var directorySource = new DirectoryInfo(source);
        var subMenuItems = new List<MenuItem>();
        MenuItem? menu = null;

        var files = GetFiles(directorySource);

        foreach (var info in files)
        {
            if (info is DirectoryInfo directoryInfo)
            {
                var (sourceEntries,subMenu) = await this.GetSources(info.FullName, baseDirectory, baseUri);
                entries.AddRange(sourceEntries);
                subMenuItems.Add(subMenu);
            }
            else if (info is FileInfo fileInfo)
            {
                if (info.Name == "README.md")
                {
                    menu = new MenuItem("README", info.FullName, this.GetUri(baseUri, baseDirectory, info.FullName));
                }
                
                if (info.Extension == ".md")
                {
                    this.logger.LogInformation("Found {mdFile}", info);
                    var modelMarkdownFile = await this.markdownTransform.GetModelMarkdownFile(fileInfo);
                    subMenuItems.Add(new MenuItem(GetTitleFromMarkdownFile(modelMarkdownFile), modelMarkdownFile.FileInfo.FullName, this.GetUri(baseUri, baseDirectory, modelMarkdownFile.FileInfo.FullName)));
                    entries.Add(modelMarkdownFile);
                }

                if (info.Extension == ".png")
                {
                    entries.Add(new Image(fileInfo));
                }
                
                if (info.Extension == ".jpeg")
                {
                    entries.Add(new Image(fileInfo));
                }
            }
        }
        
        if (menu == null)
        {
            var parentPath = Path.Combine(directorySource.FullName, "..", $"{directorySource.Name}.md");
            if (File.Exists(parentPath))
            {
                menu = new MenuItem(directorySource.Name.Humanize(), parentPath, this.GetUri(baseUri, baseDirectory, parentPath));
            }
            else
            {
                menu = new MenuItem(directorySource.Name.Humanize(), null, null);
            }
        }

        menu.Children.AddRange(subMenuItems);

        return (entries, menu);
    }

    private string GetUri(string baseUri, string baseDirectory, string infoFullName)
    {
        var path = Path.GetRelativePath(baseDirectory, infoFullName);
        
        var extension = Path.GetExtension(path);
        return $"{baseUri}/{path.Substring(0,path.Length - extension.Length)}.html";
    }

    private static FileSystemInfo[] GetFiles(DirectoryInfo directorySource)
    {
        var orderFile = Path.Combine(directorySource.FullName, ".order");
        if (File.Exists(orderFile))
        {
            var infos = File.ReadLines(orderFile)
                .Select(o => ToFileSystem(directorySource, o))
                .Where(o => o != null)
                .Cast<FileSystemInfo>()
                .ToArray();

            return
                infos
                    .Union(
                        directorySource.GetDirectories()
                            .Cast<FileSystemInfo>()
                            .Union(directorySource.GetFiles())
                            .ToArray(), 
                        new FileSystemEquality()
                        )
                    .ToArray();
        }

        return
            directorySource.GetDirectories()
                .Cast<FileSystemInfo>()
                .Union(directorySource.GetFiles())
                .ToArray();
    }

    private static FileSystemInfo? ToFileSystem(DirectoryInfo directorySource, string o)
    {
        var fileName = Path.Combine(directorySource.FullName, $"{o}.md");
        var directoryName = Path.Combine(directorySource.FullName, $"{o}");

        if (Directory.Exists(directoryName))
        {
            return new DirectoryInfo(directoryName);
        }

        if (File.Exists(fileName))
        {
            return new FileInfo(fileName);
        }
        
        return null;
    }

    private static string GetTitleFromMarkdownFile(MarkdownPage modelMarkdownFile)
    {
        var s = modelMarkdownFile.FrontMatter.Title;
        if (s != null)
        {
            return s;
        }

        return Path.GetFileNameWithoutExtension(modelMarkdownFile.FileInfo.Name)
            .Humanize();
    }

    private static async Task CopyEmbeddedFile(string destination, string name)
    {
        var assembly = typeof(BuildCommand).GetTypeInfo().Assembly;

        var resource = assembly.GetManifestResourceStream($"Neocra.Markgen.{name}");

        if (resource != null)
        {
            var file = File.OpenWrite(Path.Combine(destination, name));

            await resource.CopyToAsync(file);
            await resource.FlushAsync();
            file.Flush();
            file.Close();
        }
    }
}

internal class FileSystemEquality : IEqualityComparer<FileSystemInfo>
{
    public bool Equals(FileSystemInfo? x, FileSystemInfo? y)
    {
        return XFullName(x) == XFullName(y);
    }

    private static string? XFullName(FileSystemInfo? x)
    {
        if (x is FileInfo d)
        {
            return x?.FullName.Substring(0, x.FullName.Length - x.Extension.Length);
        }
        
        return x?.FullName;
    }

    public int GetHashCode(FileSystemInfo obj)
    {
        return 0;
    }
}