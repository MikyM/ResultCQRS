using System.Reflection;

namespace ResultCQRS;

internal static class AssemblyHelper
{
    internal static IEnumerable<Type> GetQueryImplementations(IEnumerable<Assembly> assemblies)
        => assemblies.SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(IQueryHandler)) && x.IsClass && !x.IsAbstract);
    
    internal static IEnumerable<Type> GetCommandImplementations(IEnumerable<Assembly> assemblies)
        => assemblies.SelectMany(x => x.GetTypes())
            .Where(x => x.IsAssignableTo(typeof(ICommandHandler)) && x.IsClass && !x.IsAbstract);
}
