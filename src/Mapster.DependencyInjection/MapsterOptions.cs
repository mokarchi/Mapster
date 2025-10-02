using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mapster
{
    /// <summary>
    /// Options for configuring Mapster dependency injection.
    /// </summary>
    public class MapsterOptions
    {
        /// <summary>
        /// Gets or sets the assemblies to scan for IRegister implementations and IMapFrom patterns.
        /// </summary>
        public ICollection<Assembly> AssembliesToScan { get; set; } = new List<Assembly>();

        /// <summary>
        /// Gets or sets the type pairs to precompile.
        /// </summary>
        public ICollection<(Type Source, Type Destination)> TypePairsToPrecompile { get; set; } = new List<(Type, Type)>();

        /// <summary>
        /// Gets or sets whether to use ServiceMapper (with DI support) instead of regular Mapper.
        /// Default is true.
        /// </summary>
        public bool UseServiceMapper { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to freeze the configuration after build.
        /// Frozen configs provide better performance but cannot be modified.
        /// </summary>
        public bool FreezeConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the configuration action to apply to TypeAdapterConfig.
        /// </summary>
        public Action<TypeAdapterConfig> ConfigureAction { get; set; }

        /// <summary>
        /// Gets or sets the modules to register.
        /// </summary>
        public ICollection<IMapsterModule> Modules { get; set; } = new List<IMapsterModule>();
    }
}