using Remora.Results;

namespace ResultCQRS;

/// <summary>
/// Represents a basic command handler.
/// </summary>
[PublicAPI]
public interface ICommandHandler
{
}

/// <summary>
/// Represents a command handler.
/// </summary>
[PublicAPI]
public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
{
    /// <summary>
    /// Handles the given command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
