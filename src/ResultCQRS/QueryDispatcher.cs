using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Results;

namespace ResultCQRS;

/// <inheritdoc/>
[PublicAPI]
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;

    /// <summary>
    /// Creates new instance of <see cref="QueryDispatcher"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default) where TQuery : IQuery
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
            var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a query");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result> DispatchAsync<TQuery>(TQuery query, CancellationToken cancellation = default) where TQuery : IQuery
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery>>();
            var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a query");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, IServiceProvider currentScope, CancellationToken cancellation = default) where TQuery : IQuery
    {
        try
        {
            var handler = currentScope.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
            var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result> DispatchAsync<TQuery>(TQuery query, IServiceProvider currentScope, CancellationToken cancellation = default) where TQuery : IQuery
    {
        try
        {
            var handler = currentScope.GetRequiredService<IQueryHandler<TQuery>>();
            var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a command");
            return ex;
        }
    }
}
