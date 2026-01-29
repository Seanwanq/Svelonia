using Wasmtime;

namespace Svelonia.Wasi;

public interface IWasiExtension
{
    /// <summary>
    /// Unique namespace for this extension (e.g. "svelonia", "wasi_snapshot_preview1").
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// Called when the host initializes, allowing the extension to register functions.
    /// </summary>
    /// <param name="linker">The Wasmtime linker used to define imports.</param>
    /// <param name="store">The Wasmtime store.</param>
    void Register(Linker linker, Store store);

    /// <summary>
    /// Called when the host is disposed.
    /// </summary>
    void Dispose() { }
}
