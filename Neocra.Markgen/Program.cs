using System.CommandLine;
using System.Threading.Tasks;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Domain.Markdig;
using Neocra.Markgen.Tools;
using Neocra.Markgen.Verbs.Build;
using Neocra.Markgen.Verbs.Watch;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Neocra.Markgen
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            AddServices(serviceCollection);

            var builder = serviceCollection.BuildServiceProvider();

            var handleFactory = new HandlerFactory(builder);

            var command = new RootCommand
            {
                handleFactory.Command<BuildDefinition>(),
                handleFactory.Command<WatchDefinition>(),
            };

            command.Name = "markgen";

            await command.InvokeAsync(args);
        }

        private static void AddServices(ServiceCollection services)
        {
            services.AddSingleton<HtmlEngine>();

            services.AddSingleton<MarkdownTransform>();
            services.AddSingleton<BuildCommand>();
            services.AddSingleton<WatchCommand>();
            services.AddSingleton<HtmlEngineLoader>();
            services.AddSingleton<CodeBlockRenderer>();
            services.AddSingleton<RapidocExtension>();
            services.AddSingleton<RendersProvider>();

            services.AddSingleton(new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build());
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    LogEventLevel.Debug,
                    theme:SystemConsoleTheme.None)
                .CreateLogger();
            

            services.AddLogging(c => c.AddSerilog());
        }
    }

}