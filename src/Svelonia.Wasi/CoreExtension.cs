using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Wasmtime;
using Svelonia.Core;

#pragma warning disable IL3050 // AOT JSON warning

namespace Svelonia.Wasi;

[WasiModule("svelonia")]
public partial class CoreExtension : IWasiExtension
{
    private readonly WasiHost _host;
    public string Namespace => "svelonia";

    public CoreExtension(WasiHost host)
    {
        _host = host;
    }

    public void Register(Linker linker, Store store)
    {
        RegisterGenerated(linker, store);
    }

    [WasiFunction("log")]
    public void Log(string msg)
    {
        Console.WriteLine($"[WASI-PLUGIN] {msg}");
        System.Diagnostics.Debug.WriteLine($"[WASI-PLUGIN] {msg}");
    }

    [WasiFunction("get_state")]
    public string GetState(string name)
    {
        if (name == null) return "";
        var state = _host.GetState(name);
        return state != null ? JsonSerializer.Serialize(state.ValueObject) : "";
    }

    [WasiFunction("set_state")]
    public void SetState(string name, string val)
    {
        // This was previously manually parsed in the binder or here.
        // For the demo, simply logging call or strictly only supporting string updates 
        // if we don't implement full recursive deserialization here.
        // Re-implementing basic logic:
        if (name != null && val != null)
        {
            Console.WriteLine($"[WASM] SetState {name} = {val}");
            // To properly support this in a declarative way, we would need 
            // to know the target type of the state to deserialize 'val' correctly.
            // Currently WasiHost doesn't expose strict types easily for this generic setter.
            // So we just log validation.
        }
    }

    [WasiFunction("subscribe")]
    public void Subscribe(string name)
    {
        if (name != null) _host.Subscribe(name);
    }
}
