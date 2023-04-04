using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace ResultCQRS;

/// <inheritdoc/>
[PublicAPI]
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandDispatcher> _logger;

    /// <summary>
    /// Creates new instance of <see cref="CommandDispatcher"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
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
            await using var scope = _serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
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
    public async Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, IServiceProvider currentScope, CancellationToken cancellation = default) where TCommand : ICommand
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
