using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Neocra.Markgen.Tests;

public static class SubstituteExtensions
{
    public static T AddSubstituteSingleton<T>(this IServiceCollection serviceCollection) 
        where T : class
    {
        var s = Substitute.For<T>();
        serviceCollection.AddSingleton(s);
        return s;
    }
}