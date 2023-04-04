using System.Reflection;
using AttributeBasedRegistration.Attributes.Abstractions;
using AttributeBasedRegistration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Utilities.Extensions;
using ServiceLifetime = AttributeBasedRegistration.ServiceLifetime;

namespace ResultCQRS.Autofac;

/// <summary>
/// Service collection extensions.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResultCQRS(this IServiceCollection services, params Assembly[] assembliesToScan)
        => AddResultCQRS(services, _ => {}, assembliesToScan.ToArray());
    
    public static IServiceCollection AddResultCQRS(this IServiceCollection services, Action<ResultCQRSConfiguration> options, params Assembly[] assembliesToScan)
    {
        var commandImpl = AssemblyHelper.GetCommandImplementations(assembliesToScan);
        var queryImpl = AssemblyHelper.GetQueryImplementations(assembliesToScan);

        var optionsInstance = new ResultCQRSConfiguration();
        options(optionsInstance);

        foreach (var co in commandImpl)
        {
            RegisterCommandHandler(services, co, optionsInstance);
        }
        
        foreach (var qu in queryImpl)
        {
            RegisterQueryHandler(services, qu, optionsInstance);
        }

        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

        services.AddOptions<ResultCQRSConfiguration>().Configure(options);
        
        return services;
    }

    private static void RegisterCommandHandler(IServiceCollection services, Type implementation, ResultCQRSConfiguration options)
    {
        var withoutResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(ICommandHandler<>)));
        var withResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(ICommandHandler<,>)));
        
        var lifetimeAttribute = GetLifetimeAttribute(implementation);
        var lifetime = lifetimeAttribute?.ServiceLifetime ?? options.DefaultQueryHandlerLifetime;

        if (withoutResult is not null)
            RegisterHandler(services, implementation, withoutResult, lifetime);
        
        if (withResult is not null)
            RegisterHandler(services, implementation, withResult, lifetime);
    }
    
    private static void RegisterQueryHandler(IServiceCollection services, Type implementation, ResultCQRSConfiguration options)
    {
        var withoutResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(IQueryHandler<>)));
        var withResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(IQueryHandler<,>)));
        
        var lifetimeAttribute = GetLifetimeAttribute(implementation);
        var lifetime = lifetimeAttribute?.ServiceLifetime ?? options.DefaultQueryHandlerLifetime;
        

        if (withoutResult is not null)
            RegisterHandler(services, implementation, withoutResult, lifetime);
        
        if (withResult is not null)
            RegisterHandler(services, implementation, withResult, lifetime);
    }

    private static void RegisterHandler(IServiceCollection services, Type implementation, Type serviceType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.SingleInstance:
                services.AddSingleton(serviceType, implementation);
                break;
            case ServiceLifetime.InstancePerRequest:
                services.AddScoped(serviceType, implementation);
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                services.AddScoped(serviceType, implementation);
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                throw new NotSupportedException("Not supported without using Autofac");
            case ServiceLifetime.InstancePerDependency:
                services.AddTransient(serviceType, implementation);
                break;
            case ServiceLifetime.InstancePerOwned:
                throw new NotSupportedException("Not supported without using Autofac");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static ILifetimeAttribute? GetLifetimeAttribute(Type implementation)
        => implementation.GetRegistrationAttributeOfType<ILifetimeAttribute>();
    
    internal static IEnumerable<IDecoratedByAttribute> GetDecoratorAttributes(Type implementation)
        => implementation.GetRegistrationAttributesOfType<IDecoratedByAttribute>();
    
    internal static IEnumerable<IInterceptedByAttribute> GetInterceptorAttributes(Type implementation)
        => implementation.GetRegistrationAttributesOfType<IInterceptedByAttribute>();
    
    internal static IEnableInterceptionAttribute? GetInterceptionAttribute(Type implementation)
        => implementation.GetRegistrationAttributeOfType<IEnableInterceptionAttribute>();
}
