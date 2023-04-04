using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ResultCQRS.Autofac;

/// <inheritdoc/>
[PublicAPI]
public class AutofacCommandDispatcher : ICommandDispatcher
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger<AutofacCommandDispatcher> _logger;

    /// <summary>
    /// Creates new instance of <see cref="AutofacCommandDispatcher"/>.
    /// </summary>
    /// <param name="lifetimeScope">The lifetime scope.</param>
    /// <param name="logger">The logger.</param>
    public AutofacCommandDispatcher(ILifetimeScope lifetimeScope, ILogger<AutofacCommandDispatcher> logger)
    {
        _lifetimeScope = lifetimeScope;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>
    {
        try
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope(AutofacSharedCQRSData.LifetimeScopeTag);
            var handler = scope.Resolve<ICommandHandler<TCommand, TCommandResult>>();
            var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand
    {
        try
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope(AutofacSharedCQRSData.LifetimeScopeTag);
            var handler = scope.Resolve<ICommandHandler<TCommand>>();
            var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, IServiceProvider currentScope, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>
    {
        try
        {
            var handler = currentScope.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
            var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result> DispatchAsync<TCommand>(TCommand command, IServiceProvider currentScope, CancellationToken cancellation = default) where TCommand : ICommand
    {
        try
        {
            var handler = currentScope.GetRequiredService<ICommandHandler<TCommand>>();
            var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
}
