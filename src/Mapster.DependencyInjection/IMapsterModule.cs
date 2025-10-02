namespace Mapster
{
    /// <summary>
    /// Interface for Mapster extension modules.
    /// Implement this to create reusable configuration modules.
    /// </summary>
    public interface IMapsterModule
    {
        /// <summary>
        /// Register mappings and configuration in the provided config.
        /// </summary>
        /// <param name="config">The TypeAdapterConfig to configure.</param>
        void Register(TypeAdapterConfig config);
    }
}