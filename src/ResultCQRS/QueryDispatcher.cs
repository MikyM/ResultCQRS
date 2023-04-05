using AttributeBasedRegistration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Results;

namespace ResultCQRS;

/// <inheritdoc/>
[PublicAPI]
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;
    private readonly IOptions<ResultCQRSConfiguration> _options;

    /// <summary>
    /// Creates new instance of <see cref="QueryDispatcher"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger, IOptions<ResultCQRSConfiguration> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default) where TQuery : IQuery<TQueryResult>
    {
        try
        {
            if (_options.Value.CreateScopesForQueries || (_options.Value.CreateScopeForQueriesIfCurrentIsRoot && _serviceProvider.IsRootScope()))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                
                var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
                
                var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
                
                return res;
            }
            else
            {
                var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
                
                var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
                
                return res;
            }
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
            if (_options.Value.CreateScopesForQueries || (_options.Value.CreateScopeForQueriesIfCurrentIsRoot && _serviceProvider.IsRootScope()))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                
                var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery>>();
                
                var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
                
                return res;
            }
            else
            {
                var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery>>();
                
                var res = await handler.HandleAsync(query, cancellation).ConfigureAwait(false);
                
                return res;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occured while dispatching a query");
            return ex;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, IServiceProvider scopeToUse, CancellationToken cancellation = default) where TQuery : IQuery<TQueryResult>
    {
        try
        {
            var handler = scopeToUse.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
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
    public async Task<Result> DispatchAsync<TQuery>(TQuery query, IServiceProvider scopeToUse, CancellationToken cancellation = default) where TQuery : IQuery
    {
        try
        {
            var handler = scopeToUse.GetRequiredService<IQueryHandler<TQuery>>();
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
