using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Domain.Markdig;
using Neocra.Markgen.Infrastructure;
using Neocra.Markgen.Verbs.Build;
using Neocra.Markgen.Verbs.Watch;
using Serilog;
using Serilog.Sinks.Spectre;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Neocra.Markgen
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            AddServices(serviceCollection);

            await RunAsync(serviceCollection, null, args);
        }
        
        public static Task RunAsync(ServiceCollection serviceCollection, IAnsiConsole? console, params string[] args)
        {
            var registrar = new TypeRegistrar(serviceCollection);            
            var app = new CommandApp(registrar);
            app.Configure(config =>
            {
                config.Settings.ApplicationName = "markgen";
                config.Settings.Console = console;
                config.AddCommand<BuildCommand>("build");
                config.AddCommand<WatchCommand>("warch");
                config.SetExceptionHandler(HandleException);
            });

            return app.RunAsync(args);
        }

        private static void HandleException(Exception obj)
        {
            Log.Logger.Error(obj, "Unhandle exception");
        }

        private static void AddServices(ServiceCollection services)
        {
            services.AddSingleton<HtmlEngine>();

            services.AddSingleton<IScriban, Infrastructure.Scriban>();
            services.AddSingleton<IFileWriter, FileWriter>();
            
            services.AddSingleton<MarkdownTransform>();
            services.AddSingleton<BuildCommand>();
            services.AddSingleton<WatchCommand>();
            services.AddSingleton<HtmlEngineLoader>();
            services.AddSingleton<CodeBlockRenderer>();
            services.AddSingleton<RapidocExtension>();
            services.AddSingleton<RendersProvider>();
            services.AddSingleton<UriHelper>();

            services.AddSingleton<IFileProviderFactory, PhysicalFileProviderFactory>();

            services.AddSingleton(new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build());
            
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Spectre("{Timestamp:HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();
            

            services.AddLogging(c => c.AddSerilog());
        }
    }

}