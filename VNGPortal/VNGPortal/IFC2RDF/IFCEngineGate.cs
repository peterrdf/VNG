namespace VNGPortal.IFC2RDF
{
    /// <summary>
    /// Usage of this class is to provide a synchronization gate for the IFC Engine DLL, which is not thread-safe. 
    /// All calls to the IFC Engine should be wrapped in a lock statement using the Sync object.
    /// Example:
    /// lock (IFCEngineGate.Sync)
    /// {
    ///     ...
    /// }
    /// </summary>
    internal static class IFCEngineGate
    {
        public static readonly object Sync = new();
    }
}
