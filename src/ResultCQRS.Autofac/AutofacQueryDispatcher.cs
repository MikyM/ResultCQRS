using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ResultCQRS.Autofac;

/// <inheritdoc/>
[PublicAPI]
public class AutofacQueryDispatcher : IQueryDispatcher
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger<AutofacQueryDispatcher> _logger;

    /// <summary>
    /// Creates new instance of <see cref="AutofacQueryDispatcher"/>.
    /// </summary>
    /// <param name="lifetimeScope">The lifetime scope.</param>
    /// <param name="logger">The logger.</param>
    public AutofacQueryDispatcher(ILifetimeScope lifetimeScope, ILogger<AutofacQueryDispatcher> logger)
    {
        _lifetimeScope = lifetimeScope;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default) where TQuery : IQuery<TQueryResult>
    {
        try
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope(AutofacSharedCQRSData.LifetimeScopeTag);
            var handler = scope.Resolve<IQueryHandler<TQuery, TQueryResult>>();
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
            await using var scope = _lifetimeScope.BeginLifetimeScope(AutofacSharedCQRSData.LifetimeScopeTag);
            var handler = scope.Resolve<IQueryHandler<TQuery>>();
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
    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, IServiceProvider currentScope, CancellationToken cancellation = default) where TQuery : IQuery<TQueryResult>
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
