using Remora.Results;

namespace ResultCQRS;

/// <summary>
/// Represents a command dispatcher.
/// </summary>
[PublicAPI]
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches the given command.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <typeparam name="TCommandResult">Type of the result.</typeparam>
    /// <returns>The result of the operation.</returns>
    Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>;
    
    /// <summary>
    /// Dispatches the given command.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <returns>The result of the operation.</returns>
    Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand;

    /// <summary>
    /// Dispatches the given command using the given scope.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="scope">Scope to use.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <typeparam name="TCommandResult">Type of the result.</typeparam>
    /// <returns>The result of the operation.</returns>
    Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, IServiceProvider scope, CancellationToken cancellation = default) where TCommand : ICommand<TCommandResult>;
    
    /// <summary>
    /// Dispatches the given command using the given scope.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="scope">Scope to use.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    /// <returns>The result of the operation.</returns>
    Task<Result> DispatchAsync<TCommand>(TCommand command, IServiceProvider scope, CancellationToken cancellation = default) where TCommand : ICommand;
}
