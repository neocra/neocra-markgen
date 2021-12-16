using Scriban;
using Scriban.Syntax;

namespace Neocra.Markgen.Tests;

public static class TemplateContextExtensions
{
    public static T Get<T>(this TemplateContext t, string name)
    {
        var value = t.GetValue(
            ScriptVariable.Create(name, ScriptVariableScope.Global));
        return (T)value;
    }
}