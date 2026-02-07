
/*
 * Fluid Simulation UI Control
 * Uses Svelonia.Physics.Fluid for standard Eulerian fluid dynamics.
 */
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.Composition; // For Compositor? No, ICustomDrawOperation is in SceneGraph
using Avalonia.Rendering.SceneGraph; // Correct namespace for ICustomDrawOperation
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using Svelonia.Physics.Fluid;
using System;

namespace Svelonia.Composition;

public class FluidSimControl : Control
{
    private EulerianFluidSolver? _solver;
    private IDisposable? _timer;
    private bool _isRunning = true;
    private Point _lastPos;
    private bool _isDown;

    public FluidSimControl()
    {
        // Standard Config from WebGL standards
        int simResolution = 128;
        int dyeResolution = 1440;

        _solver = new EulerianFluidSolver(simResolution, dyeResolution);

        // Tuned defaults
        _solver.CurlStrength = 30.0f;
        _solver.VelocityDissipation = 0.98f;
        _solver.DensityDissipation = 0.97f;

        _timer = DispatcherTimer.Run(() =>
        {
            if (_isRunning) InvalidateVisual();
            return _isRunning;
        }, TimeSpan.FromMilliseconds(16));

        this.PointerMoved += OnPointerMoved;
        this.PointerPressed += OnPointerPressed;
        this.PointerReleased += OnPointerReleased;
    }

    public void SetPaused(bool paused)
    {
        _isRunning = !paused;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDown = true;
        _lastPos = e.GetPosition(this);
        HandleInput(e.GetPosition(this));
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDown = false;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDown)
        {
            HandleInput(e.GetPosition(this));
        }
        else
        {
            HandleInput(e.GetPosition(this));
        }

        _lastPos = e.GetPosition(this);
    }

    public void Interact(Point p, bool isDown)
    {
        // Allow external interaction (e.g. from Canvas)
        _isDown = isDown;
        HandleInput(p);
    }

    private void HandleInput(Point pos)
    {
        if (_solver == null) return;

        double width = Bounds.Width;
        double height = Bounds.Height;
        if (width <= 0 || height <= 0) return;

        // Normalized Coords (0-1)
        float uvX = (float)(pos.X / width);
        float uvY = (float)(pos.Y / height);

        // Calculate Delta for Velocity
        float lastUvX = (float)(_lastPos.X / width);
        float lastUvY = (float)(_lastPos.Y / height);
        float deltaX = uvX - lastUvX;
        float deltaY = uvY - lastUvY;

        // Correct for Aspect Ratio
        float aspectRatio = (float)(width / height);
        if (aspectRatio < 1.0f) deltaX *= aspectRatio;
        if (aspectRatio > 1.0f) deltaY /= aspectRatio;

        // DEBUG: Coordinate Logging
        // if (_isDown) // Unconditional for debug
        {
            Console.WriteLine($"[FluidDebug] Pos: {pos}, Bounds: {width}x{height}, UV: ({uvX:F3}, {uvY:F3}), Aspect: {aspectRatio:F3}");
        }

        // Scale Factor
        float force = 6000.0f;
        float dx = deltaX * force;
        float dy = deltaY * force;

        if (dx != 0 || dy != 0)
        {
            var color = GenerateColor();
            float radius = 0.25f / 100.0f; // Normalized radius
            float intensity = 0.15f;

            // 1. Add Velocity (Physics)
            _solver.AddVelocity(uvX, uvY, dx, dy, radius);

            // 2. Add Dye (Visual)
            _solver.AddImpulse(uvX, uvY,
                (color.Red / 255f) * intensity,
                (color.Green / 255f) * intensity,
                (color.Blue / 255f) * intensity,
                radius);
        }

        _lastPos = pos;
    }

    private SKColor GenerateColor()
    {
        double time = DateTime.Now.TimeOfDay.TotalSeconds;
        float hue = (float)((time * 100) % 360);
        return SKColor.FromHsl(hue, 100, 50);
    }

    public override void Render(DrawingContext context)
    {
        // Custom Drawing via Skia
        context.Custom(new FluidCustomDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), _solver));
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    private class FluidCustomDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly EulerianFluidSolver? _solver;

        public FluidCustomDrawOperation(Rect bounds, EulerianFluidSolver? solver)
        {
            _bounds = bounds;
            _solver = solver;
        }

        public void Dispose() { }

        public bool HitTest(Point p) => false;

        public bool Equals(ICustomDrawOperation? other) => false;

        public Rect Bounds => _bounds;

        public void Render(ImmediateDrawingContext context)
        {
            if (_solver == null) return;

            var feature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
            if (feature == null) return;

            using ISkiaSharpApiLease lease = feature.Lease();
            var canvas = lease.SkCanvas;

            // Resize Logic (React Parity)
            int simBase = 128;
            int dyeBase = 1440;
            AdjustResolution(out int simW, out int simH, simBase, _bounds.Width, _bounds.Height);
            AdjustResolution(out int dyeW, out int dyeH, dyeBase, _bounds.Width, _bounds.Height);

            _solver.Resize(simW, simH, dyeW, dyeH);

            // Step Physics
            _solver.Step(0.016f, lease.GrContext);

            // Render Dye
            canvas.Clear(SKColors.Black);
            _solver.Render(canvas, SKRect.Create((float)_bounds.Width, (float)_bounds.Height));
        }

        private void AdjustResolution(out int w, out int h, int baseRes, double viewW, double viewH)
        {
            float aspectRatio = (float)(viewW / viewH);
            if (aspectRatio < 1) aspectRatio = 1.0f / aspectRatio;

            int min = baseRes;
            int max = (int)(baseRes * aspectRatio);

            if (viewW > viewH) { w = max; h = min; }
            else { w = min; h = max; }
        }
    }
}
