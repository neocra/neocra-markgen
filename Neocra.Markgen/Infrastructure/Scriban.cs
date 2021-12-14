using System.Threading.Tasks;
using Scriban;

namespace Neocra.Markgen.Infrastructure;

public class Scriban : IScriban
{
    public ValueTask<string> RenderAsync(string template, TemplateContext context)
    {

        return Template.Parse(template).RenderAsync(context);
    }
}