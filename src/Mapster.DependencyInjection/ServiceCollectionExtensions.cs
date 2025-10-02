using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;

namespace Mapster
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Mapster with default configuration (uses regular Mapper, no DI support).
        /// Preserved for backward compatibility.
        /// </summary>
        public static IServiceCollection AddMapster(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddTransient<IMapper, Mapper>();
            return serviceCollection;
        }

        /// <summary>
        /// Adds Mapster with fluent configuration options.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="configureOptions">Action to configure MapsterOptions.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMapster(
            this IServiceCollection serviceCollection,
            Action<MapsterOptions> configureOptions)
        {
            var options = new MapsterOptions();
            configureOptions?.Invoke(options);

            // Create and configure TypeAdapterConfig
            var config = new TypeAdapterConfig();
            options.ConfigureAction?.Invoke(config);

            // Build the configuration
            var builder = new MapsterBuilder(config, options);
            var buildActions = new MapsterBuildActions
            {
                ScanAssemblies = options.AssembliesToScan.Count > 0,
                PrecompileTypePairs = options.TypePairsToPrecompile.Count > 0,
                Freeze = options.FreezeConfiguration
            };
            builder.Build(buildActions);

            // Register the configuration
            serviceCollection.TryAddSingleton(config);

            // Register the appropriate mapper
            if (options.UseServiceMapper)
            {
                serviceCollection.TryAddTransient<IMapper, ServiceMapper>();
            }
            else
            {
                serviceCollection.TryAddTransient<IMapper, Mapper>();
            }

            // Register the MapContext factory
            serviceCollection.TryAddSingleton<IMapContextFactory, DefaultMapContextFactory>();

            return serviceCollection;
        }

        /// <summary>
        /// Adds Mapster with direct configuration and build actions.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="configure">Action to configure TypeAdapterConfig.</param>
        /// <param name="buildActions">Optional build actions to execute.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMapster(
            this IServiceCollection serviceCollection,
            Action<TypeAdapterConfig> configure,
            Action<MapsterBuildActions>? buildActions = null)
        {
            var config = new TypeAdapterConfig();
            configure?.Invoke(config);

            var actions = new MapsterBuildActions();
            buildActions?.Invoke(actions);

            var options = new MapsterOptions { UseServiceMapper = true };
            var builder = new MapsterBuilder(config, options);
            builder.Build(actions);

            serviceCollection.TryAddSingleton(config);
            serviceCollection.TryAddTransient<IMapper, ServiceMapper>();
            serviceCollection.TryAddSingleton<IMapContextFactory, DefaultMapContextFactory>();

            return serviceCollection;
        }

        /// <summary>
        /// Adds Mapster with an existing TypeAdapterConfig.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="existingConfig">The existing TypeAdapterConfig to use.</param>
        /// <param name="buildActions">Optional build actions to execute.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMapsterWithConfig(
            this IServiceCollection serviceCollection,
            TypeAdapterConfig existingConfig,
            Action<MapsterBuildActions>? buildActions = null)
        {
            if (existingConfig == null)
                throw new ArgumentNullException(nameof(existingConfig));

            var actions = new MapsterBuildActions();
            buildActions?.Invoke(actions);

            var options = new MapsterOptions { UseServiceMapper = true };
            var builder = new MapsterBuilder(existingConfig, options);
            builder.Build(actions);

            serviceCollection.TryAddSingleton(existingConfig);
            serviceCollection.TryAddTransient<IMapper, ServiceMapper>();
            serviceCollection.TryAddSingleton<IMapContextFactory, DefaultMapContextFactory>();

            return serviceCollection;
        }

        /// <summary>
        /// Adds Mapster with frozen configuration for high-throughput scenarios.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="configure">Action to configure TypeAdapterConfig.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMapsterFrozen(
            this IServiceCollection serviceCollection,
            Action<TypeAdapterConfig> configure)
        {
            return serviceCollection.AddMapster(configure, actions =>
            {
                actions.Freeze = true;
            });
        }

        /// <summary>
        /// Adds a Mapster module to the configuration.
        /// </summary>
        /// <typeparam name="TModule">The module type to register.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMapsterModule<TModule>(this IServiceCollection serviceCollection)
            where TModule : IMapsterModule, new()
        {
            var module = new TModule();
            var config = new TypeAdapterConfig();
            module.Register(config);

            serviceCollection.TryAddSingleton(config);
            serviceCollection.TryAddTransient<IMapper, ServiceMapper>();
            serviceCollection.TryAddSingleton<IMapContextFactory, DefaultMapContextFactory>();

            return serviceCollection;
        }

        /// <summary>
        /// Scans assemblies for IRegister and IMapFrom implementations.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection ScanMapster(
            this IServiceCollection serviceCollection,
            params Assembly[] assemblies)
        {
            return serviceCollection.AddMapster(options =>
            {
                foreach (var assembly in assemblies)
                {
                    options.AssembliesToScan.Add(assembly);
                }
                options.UseServiceMapper = true;
            });
        }
    }
}