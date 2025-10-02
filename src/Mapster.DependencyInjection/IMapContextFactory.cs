using System;

namespace Mapster
{
    /// <summary>
    /// Factory for creating MapContext scopes.
    /// </summary>
    public interface IMapContextFactory
    {
        /// <summary>
        /// Creates a new MapContext scope with the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to associate with the context.</param>
        /// <returns>A disposable MapContext scope.</returns>
        IDisposable CreateScope(IServiceProvider serviceProvider);
    }
}