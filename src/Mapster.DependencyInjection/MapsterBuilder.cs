using Mapster.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic; // added for error collection

namespace Mapster
{
    /// <summary>
    /// Internal builder for executing Mapster build phase.
    /// </summary>
    internal class MapsterBuilder
    {
        private readonly TypeAdapterConfig _config;
        private readonly MapsterOptions _options;

        public MapsterBuilder(TypeAdapterConfig config, MapsterOptions options)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Build(MapsterBuildActions? buildActions = null)
        {
            buildActions ??= new MapsterBuildActions();

            // Register modules
            foreach (var module in _options.Modules)
            {
                module.Register(_config);
            }

            // Scan assemblies if requested
            if (buildActions.ScanAssemblies && _options.AssembliesToScan.Any())
            {
                ScanAssemblies();
            }

            // Invoke before compile hook
            buildActions.OnBeforeCompile?.Invoke(_config);

            // Precompile type pairs if requested
            if (buildActions.PrecompileTypePairs && _options.TypePairsToPrecompile.Any())
            {
                PrecompileTypePairs();
            }

            // Invoke after compile hook
            buildActions.OnAfterCompile?.Invoke(_config);

            // Freeze configuration if requested
            if (buildActions.Freeze || _options.FreezeConfiguration)
            {
                FreezeConfiguration();
            }
        }

        private void ScanAssemblies()
        {
            // Scan for IRegister implementations (existing functionality)
            var assemblies = _options.AssembliesToScan.ToArray();
            _config.Scan(assemblies);

            // Scan for IMapFrom<T> implementations
            ScanForIMapFrom(assemblies);
        }

        private void ScanForIMapFrom(Assembly[] assemblies)
        {
            var mapFromInterface = typeof(IMapFrom<>);

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetLoadableTypes()
                    .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);

                foreach (var type in types)
                {
                    var interfaces = type.GetTypeInfo().GetInterfaces()
                        .Where(i => i.GetTypeInfo().IsGenericType &&
                                   i.GetGenericTypeDefinition() == mapFromInterface);

                    foreach (var @interface in interfaces)
                    {
                        var instance = Activator.CreateInstance(type);
                        var method = @interface.GetMethod("ConfigureMapping");

                        if (method != null)
                        {
                            // Call the ConfigureMapping method
                            method.Invoke(instance, new object[] { _config });
                        }
                    }
                }
            }
        }

        private void PrecompileTypePairs()
        {
            List<Exception>? errors = null;
            foreach (var (source, destination) in _options.TypePairsToPrecompile)
            {
                try
                {
                    // Force compilation by getting the map function using reflection
                    var method = typeof(TypeAdapterConfig).GetMethod("GetMapFunction",
                        BindingFlags.Instance | BindingFlags.Public)
                        ?.MakeGenericMethod(source, destination);
                    method?.Invoke(_config, Array.Empty<object>());
                }
                catch (Exception ex)
                {
                    // Collect errors instead of silently ignoring them
                    errors ??= new List<Exception>();
                    errors.Add(new InvalidOperationException($"Failed to precompile mapping from {source} to {destination}.", ex));
                }
            }

            if (errors is { Count: > 0 })
            {
                throw new AggregateException("One or more type pairs failed to precompile.", errors);
            }
        }

        private void FreezeConfiguration()
        {
            // Set the compiler to use compiled delegates
            // This improves performance by avoiding runtime compilation
            _config.Compiler = lambda => lambda.Compile();
        }
    }
}