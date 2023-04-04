namespace ResultCQRS;

/// <summary>
/// Represents a base command.
/// </summary>
[PublicAPI] 
public interface ICommandBase
{
}

/// <summary>
/// Represents a command.
/// </summary>
[PublicAPI] 
public interface ICommand : ICommandBase
{
}

/// <summary>
/// Represents a command with a concrete result.
/// </summary>
[PublicAPI] 
public interface ICommand<in TCommandResult> : ICommandBase
{
}
