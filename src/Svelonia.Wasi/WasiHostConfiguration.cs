using System.Collections.Generic;

namespace Svelonia.Wasi;

public class WasiHostConfiguration
{
    /// <summary>
    /// If set, limits the number of WebAssembly instructions executed.
    /// This prevents infinite loops in plugins.
    /// </summary>
    public long? MaxFuel { get; set; } = null;

    /// <summary>
    /// Default permissions granted to plugins if not specified during load.
    /// </summary>
    public List<string> DefaultPermissions { get; set; } = new();
}
