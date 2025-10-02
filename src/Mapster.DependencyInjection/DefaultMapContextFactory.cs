using MapsterMapper;
using System;

namespace Mapster
{
    /// <summary>
    /// Default implementation of IMapContextFactory.
    /// </summary>
    public class DefaultMapContextFactory : IMapContextFactory
    {
        public IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            var scope = new MapContextScope();
            scope.Context.Parameters[ServiceMapper.DI_KEY] = serviceProvider;
            return scope;
        }
    }
}