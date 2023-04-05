using AttributeBasedRegistration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Results;

namespace ResultCQRS;

/// <inheritdoc/>
[PublicAPI]
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandDispatcher> _logger;
    private readonly IOptions<ResultCQRSConfiguration> _options;

    /// <summary>
    /// Creates new instance of <see cref="CommandDispatcher"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger, IOptions<ResultCQRSConfiguration> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>
    {
        try
        {
            if (_options.Value.CreateScopesForCommands || (_options.Value.CreateScopeForCommandsIfCurrentIsRoot && _serviceProvider.IsRootScope()))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                
                var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
                
                var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
                
                return res;
            }
            else
            {
                var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
                
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
            if (_options.Value.CreateScopesForCommands || (_options.Value.CreateScopeForCommandsIfCurrentIsRoot && _serviceProvider.IsRootScope()))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                
                var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
                
                var res = await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
                
                return res;
            }
            else
            {
                var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
                
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
