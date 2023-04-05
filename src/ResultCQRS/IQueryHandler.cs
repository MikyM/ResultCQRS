using Remora.Results;

namespace ResultCQRS;

/// <summary>
/// Represents a basic handler.
/// </summary>
[PublicAPI]
public interface IQueryHandler
{
}


/// <summary>
/// Represents a query handler.
/// </summary>
[PublicAPI]
public interface IQueryHandler<in TQuery, TQueryResult> : IQueryHandler where TQuery : IQuery<TQueryResult>
{
    /// <summary>
    /// Handles the given query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<Result<TQueryResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
