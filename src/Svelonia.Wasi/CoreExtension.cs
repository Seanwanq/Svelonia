using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Wasmtime;
using Svelonia.Core;

#pragma warning disable IL3050 // AOT JSON warning

namespace Svelonia.Wasi;

public class CoreExtension : IWasiExtension
{
    private readonly WasiHost _host;

    public string Namespace => "svelonia";

    public CoreExtension(WasiHost host)
    {
        _host = host;
    }

    public void Register(Linker linker, Store store)
    {
        var binder = new WasiBinder(store);

        // log: string -> void
        binder.DefineAction<string>(linker, Namespace, "log", msg => Console.WriteLine($"[WASI-PLUGIN] {msg}"));

        // get_state: string -> string
        binder.DefineFunc<string, string>(linker, Namespace, "get_state", name =>
        {
            if (name == null) return "";
            var state = _host.GetState(name);
            return state != null ? JsonSerializer.Serialize(state.ValueObject) : "";
        });

        // set_state: (string, string) -> void
        binder.DefineAction<string, string>(linker, Namespace, "set_state", (name, valJson) =>
        {
            if (name != null && valJson != null)
            {
                var state = _host.GetState(name);
                if (state != null)
                {
                    try
                    {
                        var raw = valJson.Trim('"').ToLower();
                        object? parsedVal = raw;

                        if (raw == "true") parsedVal = true;
                        else if (raw == "false") parsedVal = false;
                        else if (double.TryParse(raw, out var d)) parsedVal = d;

                        state.SetValueObject(parsedVal);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WASI-HOST] Error setting state {name}: {ex.Message}");
                    }
                }
            }
        });

        // subscribe: string -> void
        binder.DefineAction<string>(linker, Namespace, "subscribe", name =>
        {
            if (name != null) _host.Subscribe(name);
        });
    }
}
