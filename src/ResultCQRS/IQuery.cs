namespace ResultCQRS;

/// <summary>
/// Represents a base query.
/// </summary>
[PublicAPI]
public interface IQueryBase
{
}

/// <summary>
/// Represents a query.
/// </summary>
[PublicAPI]
public interface IQuery : IQueryBase
{
}

/// <summary>
/// Represents a query with a concrete result.
/// </summary>
[PublicAPI]
public interface IQuery<in TQueryResult> : IQueryBase
{
}
