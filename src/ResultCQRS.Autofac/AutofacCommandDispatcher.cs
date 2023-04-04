using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ResultCQRS.Autofac;

/// <inheritdoc/>
[PublicAPI]
public class AutofacCommandDispatcher : ICommandDispatcher
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger<AutofacCommandDispatcher> _logger;
    private readonly IOptions<ResultCQRSConfiguration> _options;

    /// <summary>
    /// Creates new instance of <see cref="AutofacCommandDispatcher"/>.
    /// </summary>
    /// <param name="lifetimeScope">The lifetime scope.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public AutofacCommandDispatcher(ILifetimeScope lifetimeScope, ILogger<AutofacCommandDispatcher> logger, IOptions<ResultCQRSConfiguration> options)
    {
        _lifetimeScope = lifetimeScope;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>
    {
        try
        {
            if (_options.Value.CreateScopesForCommands)
            {
                await using var scope = _lifetimeScope.BeginLifetimeScope(AutofacSharedCQRSData.LifetimeScopeTag);
                
                var handler = scope.Resolve<ICommandHandler<TCommand, TCommandResult>>();
                
                var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
                
                return res;
            }
            else
            {
                var handler = _lifetimeScope.Resolve<ICommandHandler<TCommand, TCommandResult>>();
                
                var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
                
                return res;
            }
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
            if (_options.Value.CreateScopesForCommands)
            {
                await using var scope = _lifetimeScope.BeginLifetimeScope(AutofacSharedCQRSData.LifetimeScopeTag);
                
                var handler = scope.Resolve<ICommandHandler<TCommand>>();
                
                var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
                
                return res;
            }
            else
            {
                var handler = _lifetimeScope.Resolve<ICommandHandler<TCommand>>();
                
                var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
                
                return res;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, IServiceProvider scopeToUse, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>
    {
        try
        {
            var handler = scopeToUse.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
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
    public async Task<Result> DispatchAsync<TCommand>(TCommand command, IServiceProvider scopeToUse, CancellationToken cancellation = default) where TCommand : ICommand
    {
        try
        {
            var handler = scopeToUse.GetRequiredService<ICommandHandler<TCommand>>();
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
