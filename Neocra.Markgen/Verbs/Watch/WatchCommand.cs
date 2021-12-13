using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Neocra.Markgen.Domain;
using Neocra.Markgen.Tools;

namespace Neocra.Markgen.Verbs.Watch
{
    public class WatchCommand : IHandlerCommand<WatchOptions>
    {
        private readonly MarkdownTransform markdownTransform;

        private string source = ".";

        public WatchCommand(MarkdownTransform markdownTransform)
        {
            this.markdownTransform = markdownTransform;
        }

        public Task RunAsync(WatchOptions options)
        {
            if (!string.IsNullOrEmpty(options.Source))
            {
                this.source = options.Source;
            }
            
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(this.Configure);
                })
                .Build().RunAsync();
        }
        
        private void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                app.Use(next => async context =>
                {
                    var requestPath = context.Request.Path.ToString();

                    if (requestPath.EndsWith(".html"))
                    {
                        if (requestPath.StartsWith("/"))
                        {
                            requestPath = requestPath[1..];
                        }
                        
                        requestPath = requestPath.Replace(".html", ".md");
                        var sourceFile = Path.Combine(this.source, requestPath);

                        if (File.Exists(sourceFile))
                        {
                            var md = await File.ReadAllTextAsync(sourceFile);

                            var html = await this.markdownTransform.Transform(md);
                            context.Response.ContentType = "text/html; charset=UTF-8";
                            await context.Response.WriteAsync(html);
                            
                            return;
                        }
                    }
                    
                    await next(context);
                });
            });
        }
    }

}