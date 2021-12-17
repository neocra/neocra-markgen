using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Infrastructure;
using Neocra.Markgen.Tools;

namespace Neocra.Markgen.Verbs.Build;

public class BuildCommand : IHandlerCommand<BuildOptions>
{
    private readonly ILogger logger;
    private readonly RendersProvider rendersProvider;
    private readonly IFileProviderFactory fileProviderFactory;
    private readonly MarkdownTransform markdownTransform;
    private readonly UriHelper uriHelper;
    private readonly IFileWriter fileWriter;

    public BuildCommand(MarkdownTransform markdownTransform,
        ILogger<BuildCommand> logger,
        RendersProvider rendersProvider, 
        IFileProviderFactory fileProviderFactory,
        UriHelper uriHelper, 
        IFileWriter fileWriter)
    {
        this.markdownTransform = markdownTransform;
        this.logger = logger;
        this.rendersProvider = rendersProvider;
        this.fileProviderFactory = fileProviderFactory;
        this.uriHelper = uriHelper;
        this.fileWriter = fileWriter;
    }
        
    public async Task RunAsync(BuildOptions options)
    {
        this.logger.LogInformation("Begin build");
        var optionsSource = ".";

        if (!string.IsNullOrEmpty(options.Source))
        {
            optionsSource = options.Source;
        }
        
        var destination = ".markgen";
        if (!string.IsNullOrEmpty(options.Destination))
        {
            destination = options.Destination;
        }
        
        var directorySource = new DirectoryInfo(optionsSource);
        var physicalFileProvider =
            fileProviderFactory.GetProvider(directorySource.FullName);
        
        var (sourceEntries, menu) = await this.GetSources(physicalFileProvider, 
            "",
            optionsSource, 
            optionsSource,
            options.BaseUri ?? string.Empty);

        this.logger.LogDebug("Menu is :");

        this.LogMenu(menu, string.Empty);

        await this.rendersProvider.Renders(sourceEntries, menu, optionsSource, destination, options.BaseUri ?? string.Empty);
        
        await this.CopyEmbeddedFile(Path.Combine(destination, "resources"), "default.css");
    }
    
    private void LogMenu(MenuItem menu, string baseString)
    {
        this.logger.LogDebug(baseString + " {menu} ({path})", menu.Title, menu.FilePath);
        foreach (var menuChild in menu.Children)
        {
            this.LogMenu(menuChild, baseString + ">");
        }
    }

    private async Task<(List<Entry>, MenuItem)> GetSources(IFileProvider fileProvider, string subPath, string source, string baseDirectory, string baseUri)
    {
        var directorySource = new DirectoryInfo(source);
        var entries = new List<Entry>();
        var subMenuItems = new List<MenuItem>();
        MenuItem? menu = null;
        var files = GetFiles(fileProvider, subPath);

        foreach (var info in files)
        {
            if (info.IsDirectory)
            {
                if (info.Name == ".git")
                {
                    continue;
                }
                
                if (info.Name == ".markgen")
                {
                    continue;
                }
                
                var (sourceEntries,subMenu) = await this.GetSources(fileProvider, Path.Combine(subPath, info.Name), info.PhysicalPath, baseDirectory, baseUri);
                entries.AddRange(sourceEntries);
                subMenuItems.Add(subMenu);
            }
            else
            {
                if (info.Name == "README.md")
                {
                    menu = new MenuItem("README", info.PhysicalPath, this.uriHelper.GetUri(baseUri, baseDirectory, info.PhysicalPath));
                }
                
                if (Path.GetExtension(info.Name) == ".md")
                {
                    this.logger.LogInformation("Found {mdFile}", info.PhysicalPath);
                    var modelMarkdownFile = await this.markdownTransform.GetModelMarkdownFile(info, baseUri, baseDirectory);
                    subMenuItems.Add(modelMarkdownFile.MenuItem);
                    entries.Add(modelMarkdownFile);
                }

                if (Path.GetExtension(info.Name) == ".png")
                {
                    entries.Add(new Image(info));
                }
                
                if (Path.GetExtension(info.Name) == ".jpeg")
                {
                    entries.Add(new Image(info));
                }

                if (Path.GetExtension(info.Name) == ".css")
                {
                    entries.Add(new CssFile(info));
                }
            }
        }
        
        if (menu == null)
        {
            var parentPath = Path.Combine(directorySource.FullName, "..", $"{directorySource.Name}.md");
            if (File.Exists(parentPath))
            {
                menu = new MenuItem(directorySource.Name.Humanize(), parentPath, this.uriHelper.GetUri(baseUri, baseDirectory, parentPath));
            }
            else
            {
                menu = new MenuItem(directorySource.Name.Humanize(), null, null);
            }
        }

        menu.Children.AddRange(subMenuItems);

        return (entries, menu);
    }
    
    private static IFileInfo[] GetFiles(IFileProvider physicalFileProvider, string subpath)
    {       
        var source = physicalFileProvider;

        var orderFile = source.GetFileInfo(Path.Combine(subpath, ".order")); ;
        if (orderFile.Exists)
        {
            var infos = File.ReadLines(orderFile.PhysicalPath)
                .Select(o => ToFileSystem(source, o))
                .Where(o => o != null)
                .Select(o => (IFileInfo)o!)
                .ToArray();

            return
                infos
                    .Union(
                        source.GetDirectoryContents(subpath).ToArray(), 
                        new FileInfoEquality()
                        )
                    .ToArray();
        }

        return
            source.GetDirectoryContents(subpath).ToArray();
    }

    private static IFileInfo? ToFileSystem(IFileProvider source, string o)
    {
        var file = source.GetFileInfo($"{o}.md");
        var directory = source.GetFileInfo($"{o}");

        if (directory.Exists)
        {
            return directory;
        }

        if (file.Exists)
        {
            return file;
        }
        
        return null;
    }

    private async Task CopyEmbeddedFile(string destination, string name)
    {
        var assembly = typeof(BuildCommand).GetTypeInfo().Assembly;

        var resource = assembly.GetManifestResourceStream($"Neocra.Markgen.Template.{name}");

        if (resource != null)
        {
            var content = await new StreamReader(resource).ReadToEndAsync();

            fileWriter.CreateDirectory(destination);
            await this.fileWriter.WriteAllTextAsync( Path.Combine(destination, name), content);
        }
    }
}