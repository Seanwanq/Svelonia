using System;
using Wasmtime;
using Svelonia.Core;

namespace Svelonia.Wasi;

public class DrawingExtension : IWasiExtension
{
    private readonly WasiHost _host;
    public string Namespace => "svelonia";

    public event Action<DrawCommand>? OnDrawCommand;

    public DrawingExtension(WasiHost host)
    {
        _host = host;
    }

    public void Register(Linker linker, Store store)
    {
        var binder = new WasiBinder(store);

        // draw_begin_path: (id, color, thickness)
        binder.DefineDrawBegin(linker, Namespace, "draw_begin_path", (id, color, thickness) =>
        {
            if (id != null && color != null)
                OnDrawCommand?.Invoke(new DrawCommand(DrawOp.BeginPath, id, 0, 0, color, thickness));
        });

        // draw_add_point: (id, x, y)
        // Need overload for (string, double, double)
        // Let's instantiate binder generic or use manual for now if binder is limited.
        // Or update binder to generic handling. 
        // For this demo, let's use the explicit pattern since C# generics with variable args 
        // (ptr,len vs val) is hard without expression trees.

        linker.Define(Namespace, "draw_add_point", Function.FromCallback(store, (Caller caller, int idPtr, int idLen, double x, double y) =>
        {
            // Using helper to read string is at least better
            var mem = caller.GetMemory("memory");
            var id = mem?.ReadString(idPtr, idLen);
            if (id != null)
                OnDrawCommand?.Invoke(new DrawCommand(DrawOp.AddPoint, id, x, y));
        }));

        // draw_end_path: String
        binder.DefineAction<string>(linker, Namespace, "draw_end_path", (id) =>
        {
            if (id != null)
                OnDrawCommand?.Invoke(new DrawCommand(DrawOp.EndPath, id, 0, 0));
        });
    }

    public void SimulateDraw(DrawCommand cmd) => OnDrawCommand?.Invoke(cmd);
}
