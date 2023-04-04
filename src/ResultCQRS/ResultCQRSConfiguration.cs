using AttributeBasedRegistration;

namespace ResultCQRS;

/// <summary>
/// Represents the configuration of the library.
/// </summary>
[PublicAPI]
public sealed class ResultCQRSConfiguration
{
    /// <summary>
    /// Gets or sets the default lifetime of a command handler.
    /// </summary>
    public ServiceLifetime DefaultCommandHandlerLifetime { get; set; } = ServiceLifetime.InstancePerLifetimeScope;
    /// <summary>
    /// Gets or sets the default lifetime of a query handler.
    /// </summary>
    public ServiceLifetime DefaultQueryHandlerLifetime { get; set; } = ServiceLifetime.InstancePerLifetimeScope;
    /// <summary>
    /// Gets or sets the default lifetime of a command dispatcher.
    /// </summary>
    public ServiceLifetime DefaultCommandDispatcherLifetime { get; set; } = ServiceLifetime.SingleInstance;
    /// <summary>
    /// Gets or sets the default lifetime of a query dispatcher.
    /// </summary>
    public ServiceLifetime DefaultQueryDispatcherLifetime { get; set; } = ServiceLifetime.SingleInstance;
    /// <summary>
    /// Gets or sets whether to create a child scope for every dispatched query.
    /// </summary>
    public bool CreateScopesForQueries { get; set; } = true;
    /// <summary>
    /// Gets or sets whether to create a child scope for every dispatched command.
    /// </summary>
    public bool CreateScopesForCommands { get; set; } = true;
}
