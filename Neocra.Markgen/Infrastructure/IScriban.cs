using System.Threading.Tasks;
using Scriban;

namespace Neocra.Markgen.Infrastructure;

public interface IScriban
{
    ValueTask<string> RenderAsync(string template, TemplateContext context);
}