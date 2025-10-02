using System;

namespace Mapster
{
    /// <summary>
    /// Actions to be executed during Mapster build phase.
    /// </summary>
    public class MapsterBuildActions
    {
        /// <summary>
        /// Gets or sets the action to execute before compilation.
        /// </summary>
        public Action<TypeAdapterConfig> OnBeforeCompile { get; set; }

        /// <summary>
        /// Gets or sets the action to execute after compilation.
        /// </summary>
        public Action<TypeAdapterConfig> OnAfterCompile { get; set; }

        /// <summary>
        /// Gets or sets whether to scan assemblies for IRegister and IMapFrom implementations.
        /// </summary>
        public bool ScanAssemblies { get; set; }

        /// <summary>
        /// Gets or sets whether to precompile type pairs.
        /// </summary>
        public bool PrecompileTypePairs { get; set; }

        /// <summary>
        /// Gets or sets whether to freeze the configuration.
        /// </summary>
        public bool Freeze { get; set; }
    }
}