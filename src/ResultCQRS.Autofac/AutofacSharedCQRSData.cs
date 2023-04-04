namespace ResultCQRS.Autofac;

/// <summary>
/// Shared, Autofac related, library data.
/// </summary>
[PublicAPI]
public static class AutofacSharedCQRSData
{
    /// <summary>
    /// The lifetime scope tag used to create child Autofac scopes.
    /// </summary>
    public const string LifetimeScopeTag = "ResultCQRS";
}
