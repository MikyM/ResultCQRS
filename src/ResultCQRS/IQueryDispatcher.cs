using Remora.Results;

namespace ResultCQRS;

/// <summary>
/// Represents a query dispatcher.
/// </summary>
[PublicAPI]
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches the given query.
    /// </summary>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <typeparam name="TQuery">Type of the query.</typeparam>
    /// <typeparam name="TQueryResult">Type of the result.</typeparam>
    /// <returns>The result of the operation.</returns>
    Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default) where TQuery : IQuery<TQueryResult>;
}
