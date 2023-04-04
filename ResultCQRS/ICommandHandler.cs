using Remora.Results;

namespace ResultCQRS;

/// <summary>
/// Represents a query handler.
/// </summary>
[PublicAPI]
public interface IQueryHandler<in TQuery, TQueryResult>
{
    Task<Result<TQueryResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
