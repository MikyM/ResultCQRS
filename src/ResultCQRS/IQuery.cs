namespace ResultCQRS;

/// <summary>
/// Represents a query with a concrete result.
/// </summary>
[PublicAPI]
public interface IQuery<in TQueryResult>
{
}
