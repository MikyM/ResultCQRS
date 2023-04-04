using Remora.Results;

namespace ResultCQRS;

/// <summary>
/// Represents a command dispatcher.
/// </summary>
[PublicAPI]
public interface ICommandDispatcher
{
    Task<Result<TCommandResult>> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand;
}
