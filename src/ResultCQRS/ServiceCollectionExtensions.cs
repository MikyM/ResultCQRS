using System.Reflection;
using AttributeBasedRegistration;
using AttributeBasedRegistration.Attributes.Abstractions;
using AttributeBasedRegistration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Utilities.Extensions;
using ServiceLifetime = AttributeBasedRegistration.ServiceLifetime;

namespace ResultCQRS;

/// <summary>
/// Service collection extensions.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default implementations of dispatchers and all found handlers within the provided assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembliesToScan">The assemblies to scan for handlers.</param>
    /// <returns>Current instance of <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddResultCQRS(this IServiceCollection services, params Assembly[] assembliesToScan)
        => AddResultCQRS(services, _ => {}, assembliesToScan.ToArray());

    /// <summary>
    /// Registers the default implementations of dispatchers and all found handlers within the provided assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The configuration.</param>
    /// <param name="assembliesToScan">The assemblies to scan for handlers.</param>
    /// <returns>Current instance of <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddResultCQRS(this IServiceCollection services, Action<ResultCQRSConfiguration> options, params Assembly[] assembliesToScan)
    {
        var commandImpl = AssemblyHelper.GetCommandImplementations(assembliesToScan);
        var queryImpl = AssemblyHelper.GetQueryImplementations(assembliesToScan);

        var optionsInstance = new ResultCQRSConfiguration();
        options(optionsInstance);

        services.AddRootScopeIdentifier();

        foreach (var co in commandImpl)
        {
            RegisterCommandHandler(services, co, optionsInstance);
        }
        
        foreach (var qu in queryImpl)
        {
            RegisterQueryHandler(services, qu, optionsInstance);
        }

        switch (optionsInstance.DefaultCommandDispatcherLifetime)
        {
            case ServiceLifetime.SingleInstance:
                services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
                break;
            case ServiceLifetime.InstancePerRequest:
                services.AddScoped<ICommandDispatcher, CommandDispatcher>();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                services.AddScoped<ICommandDispatcher, CommandDispatcher>();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                throw new NotSupportedException();
            case ServiceLifetime.InstancePerDependency:
                services.AddTransient<ICommandDispatcher, CommandDispatcher>();
                break;
            case ServiceLifetime.InstancePerOwned:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        switch (optionsInstance.DefaultCommandDispatcherLifetime)
        {
            case ServiceLifetime.SingleInstance:
                services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
                break;
            case ServiceLifetime.InstancePerRequest:
                services.AddScoped<IQueryDispatcher, QueryDispatcher>();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                services.AddScoped<IQueryDispatcher, QueryDispatcher>();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                throw new NotSupportedException();
            case ServiceLifetime.InstancePerDependency:
                services.AddTransient<IQueryDispatcher, QueryDispatcher>();
                break;
            case ServiceLifetime.InstancePerOwned:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

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
