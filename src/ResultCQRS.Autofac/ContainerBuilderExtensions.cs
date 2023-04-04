using System.Reflection;
using AttributeBasedRegistration.Attributes.Abstractions;
using AttributeBasedRegistration.Autofac;
using AttributeBasedRegistration.Extensions;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.Options;
using MikyM.Utilities.Extensions;
using ServiceLifetime = AttributeBasedRegistration.ServiceLifetime;

namespace ResultCQRS.Autofac;

/// <summary>
/// Service collection extensions.
/// </summary>
[PublicAPI]
public static class ContainerBuilderExtensions
{
    /// <summary>
    /// Registers the default implementations of dispatchers and all found handlers within the provided assemblies.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="assembliesToScan">The assemblies to scan for handlers.</param>
    /// <returns>Current instance of <see cref="ContainerBuilder"/>.</returns>
    public static ContainerBuilder AddResultCQRS(this ContainerBuilder builder, params Assembly[] assembliesToScan)
        => AddResultCQRS(builder, _ => {}, assembliesToScan.ToArray());

    /// <summary>
    /// Registers the default implementations of dispatchers and all found handlers within the provided assemblies.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="options">The configuration.</param>
    /// <param name="assembliesToScan">The assemblies to scan for handlers.</param>
    /// <returns>Current instance of <see cref="ContainerBuilder"/>.</returns>
    public static ContainerBuilder AddResultCQRS(this ContainerBuilder builder, Action<ResultCQRSConfiguration> options, params Assembly[] assembliesToScan)
    {
        var commandImpl = AssemblyHelper.GetCommandImplementations(assembliesToScan);
        var queryImpl = AssemblyHelper.GetQueryImplementations(assembliesToScan);

        var optionsInstance = new ResultCQRSConfiguration();
        options(optionsInstance);

        foreach (var co in commandImpl)
        {
            RegisterCommandHandler(builder, co, optionsInstance);
        }
        
        foreach (var qu in queryImpl)
        {
            RegisterQueryHandler(builder, qu, optionsInstance);
        }

        switch (optionsInstance.DefaultCommandDispatcherLifetime)
        {
            case ServiceLifetime.SingleInstance:
                builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>().SingleInstance();
                break;
            case ServiceLifetime.InstancePerRequest:
                builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>().InstancePerRequest();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>().InstancePerLifetimeScope();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>().InstancePerMatchingLifetimeScope();
                break;
            case ServiceLifetime.InstancePerDependency:
                builder.RegisterType<CommandDispatcher>().As<ICommandDispatcher>().InstancePerDependency();
                break;
            case ServiceLifetime.InstancePerOwned:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        switch (optionsInstance.DefaultCommandDispatcherLifetime)
        {
            case ServiceLifetime.SingleInstance:
                builder.RegisterType<QueryDispatcher>().As<IQueryDispatcher>().SingleInstance();
                break;
            case ServiceLifetime.InstancePerRequest:
                builder.RegisterType<QueryDispatcher>().As<IQueryDispatcher>().InstancePerRequest();
                break;
            case ServiceLifetime.InstancePerLifetimeScope:
                builder.RegisterType<QueryDispatcher>().As<IQueryDispatcher>().InstancePerLifetimeScope();
                break;
            case ServiceLifetime.InstancePerMatchingLifetimeScope:
                builder.RegisterType<QueryDispatcher>().As<IQueryDispatcher>().InstancePerMatchingLifetimeScope();
                break;
            case ServiceLifetime.InstancePerDependency:
                builder.RegisterType<QueryDispatcher>().As<IQueryDispatcher>().InstancePerDependency();
                break;
            case ServiceLifetime.InstancePerOwned:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

        builder.RegisterInstance(optionsInstance).SingleInstance().AsSelf();
        var opt = Options.Create(optionsInstance);
        builder.RegisterInstance(opt).As<IOptions<ResultCQRSConfiguration>>().SingleInstance();

        return builder;
    }

    private static void RegisterCommandHandler(ContainerBuilder builder, Type implementation, ResultCQRSConfiguration options)
    {
        if (implementation.GetRegistrationAttributesOfType<ISkipRegistrationAttribute>().Any())
            return;
        
        var withoutResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(ICommandHandler<>)));
        var withResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(ICommandHandler<,>)));
        
        var lifetimeAttribute = GetLifetimeAttribute(implementation);
        var lifetime = lifetimeAttribute?.ServiceLifetime ?? options.DefaultQueryHandlerLifetime;

        var interceptionAttribute = GetInterceptionAttribute(implementation);
        var interceptedByAttributes = GetInterceptorAttributes(implementation).ToArray();
        var decoratedByAttributes = GetDecoratorAttributes(implementation).ToArray();

        if (withoutResult is not null)
            RegisterHandler(builder, implementation, withoutResult, lifetime, interceptionAttribute, interceptedByAttributes, decoratedByAttributes);
        
        if (withResult is not null)
            RegisterHandler(builder, implementation, withResult, lifetime, interceptionAttribute, interceptedByAttributes, decoratedByAttributes);
    }
    
    private static void RegisterQueryHandler(ContainerBuilder builder, Type implementation, ResultCQRSConfiguration options)
    {
        if (implementation.GetRegistrationAttributesOfType<ISkipRegistrationAttribute>().Any())
            return;
        
        var withoutResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(IQueryHandler<>)));
        var withResult = implementation.GetInterfaces().FirstOrDefault(x => x.IsAssignableToWithGenerics(typeof(IQueryHandler<,>)));
        
        var lifetimeAttribute = GetLifetimeAttribute(implementation);
        var lifetime = lifetimeAttribute?.ServiceLifetime ?? options.DefaultQueryHandlerLifetime;
        
        var interceptionAttribute = GetInterceptionAttribute(implementation);
        var interceptedByAttributes = GetInterceptorAttributes(implementation).ToArray();
        var decoratedByAttributes = GetDecoratorAttributes(implementation).ToArray();
        
        if (withoutResult is not null)
            RegisterHandler(builder, implementation, withoutResult, lifetime, interceptionAttribute, interceptedByAttributes, decoratedByAttributes);
        
        if (withResult is not null)
            RegisterHandler(builder, implementation, withResult, lifetime, interceptionAttribute, interceptedByAttributes, decoratedByAttributes);
    }

    private static void RegisterHandler(ContainerBuilder builder, Type implementation, Type serviceType, ServiceLifetime lifetime, 
        IEnableInterceptionAttribute? interceptionAttribute, IInterceptedByAttribute[] interceptedByAttributes, IDecoratedByAttribute[] decoratedByAttributes)
    {
        var innerBuilder = lifetime switch
            {
                ServiceLifetime.SingleInstance => builder.RegisterType(implementation).As(serviceType).SingleInstance(),
                ServiceLifetime.InstancePerRequest => builder.RegisterType(implementation)
                    .As(serviceType)
                    .InstancePerRequest(),
                ServiceLifetime.InstancePerLifetimeScope => builder.RegisterType(implementation)
                    .As(serviceType)
                    .InstancePerLifetimeScope(),
                ServiceLifetime.InstancePerMatchingLifetimeScope => builder.RegisterType(implementation)
                    .As(serviceType)
                    .InstancePerMatchingLifetimeScope(),
                ServiceLifetime.InstancePerDependency => builder.RegisterType(implementation)
                    .As(serviceType)
                    .InstancePerDependency(),
                ServiceLifetime.InstancePerOwned => throw new NotSupportedException(
                    "Not supported without using Autofac"),
                _ => throw new ArgumentOutOfRangeException()
            };

        if (interceptionAttribute is not null)
        {
            innerBuilder.EnableInterfaceInterceptors();

            if (interceptedByAttributes.GroupBy(x => x.RegistrationOrder).FirstOrDefault(x => x.Count() > 1) is not null)
                throw new InvalidOperationException($"Duplicated interceptor registration order on type {implementation.Name}");

            if (interceptedByAttributes.GroupBy(x => x.Interceptor)
                    .FirstOrDefault(x => x.Count() > 1) is not null)
                throw new InvalidOperationException($"Duplicated interceptor type on type {implementation.Name}");
            
            foreach (var interceptor in interceptedByAttributes.OrderByDescending(x => x.RegistrationOrder).Select(x => x.Interceptor).Distinct())
            {
                innerBuilder = interceptor.IsAsyncInterceptor()
                    ? innerBuilder.InterceptedBy(
                        typeof(AsyncInterceptorAdapter<>).MakeGenericType(interceptor))
                    : innerBuilder.InterceptedBy(interceptor);
            }
        }
        
        if (decoratedByAttributes.GroupBy(x => x.RegistrationOrder).FirstOrDefault(x => x.Count() > 1) is not null)
            throw new InvalidOperationException($"Duplicated decorator registration order on type {implementation.Name}");

        if (decoratedByAttributes.GroupBy(x => x.Decorator)
                .FirstOrDefault(x => x.Count() > 1) is not null)
            throw new InvalidOperationException($"Duplicated decorator type on type {implementation.Name}");

        foreach (var attribute in decoratedByAttributes.OrderBy(x => x.RegistrationOrder))
        {
            if (attribute.Decorator.ShouldSkipRegistration<ISkipDecoratorRegistrationAttribute>())
                continue;
            
            if (attribute.Decorator.IsGenericType && attribute.Decorator.IsGenericTypeDefinition)
                builder.RegisterGenericDecorator(attribute.Decorator, serviceType);
            else
                builder.RegisterDecorator(attribute.Decorator, serviceType);
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
