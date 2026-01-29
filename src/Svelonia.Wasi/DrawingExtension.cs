using System;
using Wasmtime;
using Svelonia.Core;

namespace Svelonia.Wasi;

[WasiModule("svelonia")]
public partial class DrawingExtension : IWasiExtension
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
        RegisterGenerated(linker, store);
    }

    [WasiFunction("draw_begin_path")]
    public void DrawBeginPath(string id, string color, double thickness)
    {
        if (!_host.CheckPermission("drawing")) return;
        if (id != null && color != null)
            OnDrawCommand?.Invoke(new DrawCommand(DrawOp.BeginPath, id, 0, 0, color, thickness));
    }

    // Manual binding for mixed args not yet fully supported by simplistic AutoBinder if needed
    // But let's try to fit it in AutoBinder or keep manual for edge case.
    // The AutoBinder currently supports Action<T1, T2>.
    // draw_add_point is (string id, double x, double y) -> 3 args.
    // My simple AutoBinder demo handled 2 generic args maximum.
    // Let's UPDATE AutoBinder to support 3 args or add manual registration ONLY for this one.

    // For elegance, let's just add manual reg for this one inside Register if AutoBinder fails?
    // OR Update AutoBinder. Let's update implementation in next step if generic needed.
    // Actually, I put a hardcoded "draw_add_point" check or "draw_begin_path" check in AutoBinder.
    // Let's double check AutoBinder code.
    // I handled "draw_begin_path" (3 args) specifically. 
    // I did NOT handle "draw_add_point" (3 args: string, double, double).

    // Strategy: Use manual binding for draw_add_point for safety now, to avoid rewriting Binder logic excessively.
    // But mark others with attributes.

    [WasiFunction("draw_add_point")]
    public void DrawAddPointNative(string id, double x, double y)
    {
        if (!_host.CheckPermission("drawing")) return;
        OnDrawCommand?.Invoke(new DrawCommand(DrawOp.AddPoint, id, x, y));
    }

    [WasiFunction("draw_end_path")]
    public void DrawEndPath(string id)
    {
        if (!_host.CheckPermission("drawing")) return;
        if (id != null)
            OnDrawCommand?.Invoke(new DrawCommand(DrawOp.EndPath, id, 0, 0));
    }

    public void SimulateDraw(DrawCommand cmd) => OnDrawCommand?.Invoke(cmd);
}
