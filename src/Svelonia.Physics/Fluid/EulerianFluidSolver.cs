
using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Collections.Concurrent;

namespace Svelonia.Physics.Fluid;

/// <summary>
/// A grid-based (Eulerian) fluid solver for real-time visual effects.
/// Solves the Incompressible Navier-Stokes equations using the Stable Fluids method.
/// </summary>
public class EulerianFluidSolver : IDisposable
{
    private class PingPongBuffer : IDisposable
    {
        public SKSurface? Read;
        public SKSurface? Write;
        private readonly int _width;
        private readonly int _height;

        public PingPongBuffer(int width, int height) { _width = width; _height = height; }

        public void EnsureInitialized(GRContext? context)
        {
            if (Read != null && Read.Context == context) return;
            Dispose();

            var info = new SKImageInfo(_width, _height, SKColorType.RgbaF16, SKAlphaType.Premul);
            if (context != null)
            {
                Read = SKSurface.Create(context, true, info);
                Write = SKSurface.Create(context, true, info);
            }
            else
            {
                Read = SKSurface.Create(info);
                Write = SKSurface.Create(info);
            }

            // Init to zero/black
            Read.Canvas.Clear(SKColors.Black);
            Write.Canvas.Clear(SKColors.Black);
        }

        public void Swap() => (Read, Write) = (Write, Read);
        public void Dispose() { Read?.Dispose(); Write?.Dispose(); }
    }

    // State Buffers
    private PingPongBuffer _velocity;
    private PingPongBuffer _density; // "Dye"
    private PingPongBuffer _pressure;
    private SKSurface? _divergence;
    private SKSurface? _curl;

    // Simulation Parameters
    public float Viscosity { get; set; } = 0.0f; // Usually ignored for visual smoke
    public float DensityDissipation { get; set; } = 0.98f;
    public float VelocityDissipation { get; set; } = 0.99f;
    public float CurlStrength { get; set; } = 20.0f;
    public int PressureIterations { get; set; } = 20;

    // Grid Config
    private int _simWidth;
    private int _simHeight;
    private int _densityWidth;
    private int _densityHeight;

    // Shader Cache
    private SKRuntimeEffect _sAddImpulse = null!;
    private SKRuntimeEffect _sAdvection = null!;
    private SKRuntimeEffect _sDivergence = null!;
    private SKRuntimeEffect _sCurl = null!;
    private SKRuntimeEffect _sVorticity = null!;
    private SKRuntimeEffect _sJacobi = null!; // Pressure
    private SKRuntimeEffect _sSubtractGradient = null!;
    private SKRuntimeEffect _sDisplay = null!;

    // Input Queue
    public struct Impulse { public float X, Y; public float R, G, B; public float Radius; public bool IsVelocity; }
    private ConcurrentQueue<Impulse> _impulses = new();

    public EulerianFluidSolver(int simRes, int densityRes)
    {
        _simWidth = simRes; // Temp init, expected to be resized or use square defaults
        _simHeight = simRes;
        _densityWidth = densityRes;
        _densityHeight = densityRes;

        _velocity = new PingPongBuffer(_simWidth, _simHeight);
        _pressure = new PingPongBuffer(_simWidth, _simHeight);
        _density = new PingPongBuffer(_densityWidth, _densityHeight); // Dye can be higher res

        CompileShaders();
    }

    public void Resize(int simWidth, int simHeight, int densityWidth, int densityHeight)
    {
        if (_simWidth == simWidth && _simHeight == simHeight && 
            _densityWidth == densityWidth && _densityHeight == densityHeight) 
            return;

        _simWidth = simWidth;
        _simHeight = simHeight;
        _densityWidth = densityWidth;
        _densityHeight = densityHeight;

        // Dispose old buffers
        _velocity?.Dispose();
        _pressure?.Dispose();
        _density?.Dispose();
        _divergence?.Dispose(); _divergence = null;
        _curl?.Dispose(); _curl = null;

        // Create new buffers
        _velocity = new PingPongBuffer(_simWidth, _simHeight);
        _pressure = new PingPongBuffer(_simWidth, _simHeight);
        _density = new PingPongBuffer(_densityWidth, _densityHeight);
        
        // Note: divergence and curl will be recreated in EnsureSurfaces or Step
    }

    private void CompileShaders()
    {
        _sAddImpulse = Compile(FluidEquations.Impulse);
        _sAdvection = Compile(FluidEquations.Advection);
        _sCurl = Compile(FluidEquations.Curl);
        _sVorticity = Compile(FluidEquations.Vorticity);
        _sDivergence = Compile(FluidEquations.Divergence);
        _sJacobi = Compile(FluidEquations.Jacobi);
        _sSubtractGradient = Compile(FluidEquations.SubtractGradient);
        _sDisplay = Compile(FluidEquations.Display);
    }

    private static SKRuntimeEffect Compile(string sksl)
    {
        var effect = SKRuntimeEffect.Create(sksl, out string error);
        if (effect == null) throw new InvalidOperationException($"SkSL Compile Error: {error}");
        return effect;
    }

    public void AddImpulse(float x, float y, float r, float g, float b, float radius)
    {
        _impulses.Enqueue(new Impulse { X = x, Y = y, R = r, G = g, B = b, Radius = radius, IsVelocity = false });
    }

    public void AddVelocity(float x, float y, float dx, float dy, float radius)
    {
        // Enqueue Velocity Splat instead of immediate drawing (Thread Safety)
        _impulses.Enqueue(new Impulse { X = x, Y = y, R = dx, G = dy, B = 0, Radius = radius, IsVelocity = true });
    }

    public void Step(float dt, GRContext? context)
    {
        EnsureSurfaces(context);

        // Frame-Scope Resource Management
        // Creating a disposable container for all intermediate snapshots/shaders used in this frame
        using var frameResources = new CompositeDisposable();

        // Aspect Ratio helps keep splats circular
        float aspectRatio = (float)_densityWidth / _densityHeight;

        // 0. Process Inputs
        while (_impulses.TryDequeue(out var p))
        {
            ApplyImpulse(p, aspectRatio, frameResources);
        }

        // 1. Curl + Vorticity Confinement
        // Compute Curl from Velocity
        Dispatch(_sCurl, _velocity.Read!, _curl!, _simWidth, _simHeight, frameResources, null);

        // Add Vorticity Force to Velocity
        var vortUniforms = new SKRuntimeEffectUniforms(_sVorticity)
        {
            ["uCurlStrength"] = CurlStrength,
            ["uDt"] = dt
        };
        Dispatch(_sVorticity, _velocity.Read!, _velocity.Write!, _simWidth, _simHeight, frameResources, vortUniforms, new Dictionary<string, SKSurface> { ["uCurl"] = _curl! });
        _velocity.Swap();

        // 2. Divergence (Velocity)
        Dispatch(_sDivergence, _velocity.Read!, _divergence!, _simWidth, _simHeight, frameResources, null);

        // 3. Pressure Solve (Jacobi)
        _pressure.Write!.Canvas.Clear(SKColors.Black); // Initial guess 0
        _pressure.Swap();

        for (int i = 0; i < PressureIterations; i++)
        {
            var pInputs = new Dictionary<string, SKSurface>
            {
                ["uPressure"] = _pressure.Read!, // Neighbors
                ["uDivergence"] = _divergence!   // b-vector
            };
            Dispatch(_sJacobi, null, _pressure.Write!, _simWidth, _simHeight, frameResources, null, pInputs);
            _pressure.Swap();
        }

        // 4. Gradient Subtraction (Project)
        // Velocity - Grad(Pressure)
        var subInputs = new Dictionary<string, SKSurface>
        {
            ["uPressure"] = _pressure.Read!,
            ["uVelocity"] = _velocity.Read!
        };
        Dispatch(_sSubtractGradient, null, _velocity.Write!, _simWidth, _simHeight, frameResources, null, subInputs);
        _velocity.Swap();

        // 5. Advect Velocity (Self-Advection)
        // u_new = u_old at (x - u*dt)
        var advVelUniforms = new SKRuntimeEffectUniforms(_sAdvection)
        {
            ["uDt"] = dt,
            ["uDissipation"] = VelocityDissipation
        };
        var advVelInputs = new Dictionary<string, SKSurface>
        {
            ["uVelocity"] = _velocity.Read!,
            ["uSource"] = _velocity.Read!
        };
        Dispatch(_sAdvection, null, _velocity.Write!, _simWidth, _simHeight, frameResources, advVelUniforms, advVelInputs);
        _velocity.Swap();

        // 6. Advect Density (Dye)
        var advDyeUniforms = new SKRuntimeEffectUniforms(_sAdvection)
        {
            ["uDt"] = dt,
            ["uDissipation"] = DensityDissipation
        };
        var advDyeInputs = new Dictionary<string, SKSurface>
        {
            ["uVelocity"] = _velocity.Read!, // Advected by Velocity field
            ["uSource"] = _density.Read!     // Moving the Dye
        };
        Dispatch(_sAdvection, null, _density.Write!, _densityWidth, _densityHeight, frameResources, advDyeUniforms, advDyeInputs);
        _density.Swap();

        // CRITICAL: Flush to ensure all GPU commands submitted before 'frameResources' disposes textures
        // We can flush the context if we have it, or just the last surface.
        if (context != null) context.Flush();
        else _density.Read!.Canvas.Flush();
    }

    public void Render(SKCanvas canvas, SKRect destRect)
    {
        if (_density.Read == null) return;

        using var paint = new SKPaint();
        using var snapshot = _density.Read.Snapshot();
        using var shader = snapshot.ToShader(); // Standard texture mapping

        // Simple display shader
        paint.Shader = shader;
        paint.FilterQuality = SKFilterQuality.High;

        canvas.DrawRect(destRect, paint);
    }

    private void ApplyImpulse(Impulse imp, float aspect, CompositeDisposable resources)
    {
        var uniforms = new SKRuntimeEffectUniforms(_sAddImpulse)
        {
            ["uPoint"] = new[] { imp.X, imp.Y },
            ["uColor"] = new[] { imp.R, imp.G, imp.B },
            ["uRadius"] = imp.Radius,
            ["uAspectRatio"] = imp.IsVelocity ? (float)_simWidth / _simHeight : aspect
        };

        if (imp.IsVelocity)
        {
            Dispatch(_sAddImpulse, _velocity.Read!, _velocity.Write!, _simWidth, _simHeight, resources, uniforms);
            _velocity.Swap();
        }
        else
        {
            Dispatch(_sAddImpulse, _density.Read!, _density.Write!, _densityWidth, _densityHeight, resources, uniforms);
            _density.Swap();
        }
    }

    private void Dispatch(SKRuntimeEffect effect, SKSurface? uniformSource, SKSurface target,
                          int width, int height,
                          CompositeDisposable resources,
                          SKRuntimeEffectUniforms? uniforms = null,
                          Dictionary<string, SKSurface>? textureInputs = null)
    {
        using var paint = new SKPaint { BlendMode = SKBlendMode.Src };

        // Resolution Uniform
        if (uniforms == null) uniforms = new SKRuntimeEffectUniforms(effect);
        if (effect.Uniforms.Contains("uTexelSize"))
        {
            uniforms["uTexelSize"] = new[] { 1.0f / width, 1.0f / height };
        }

        // Texture Bindings
        var children = new SKRuntimeEffectChildren(effect);
        // Resources are tracked in 'resources' (Frame Scope)

        // 1. Explicit inputs
        if (textureInputs != null)
        {
            foreach (var input in textureInputs)
            {
                if (input.Value != null)
                {
                    var img = input.Value.Snapshot();
                    resources.Add(img);
                    var shader = img.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
                    resources.Add(shader);
                    children[input.Key] = shader;
                }
            }
        }

        // 2. Implicit "Self" or "Source" binding
        if (uniformSource != null)
        {
            var img = uniformSource.Snapshot();
            resources.Add(img);
            var shader = img.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
            resources.Add(shader);

            // Bind to common names if present and not bound
            if (effect.Children.Contains("uVelocity") && !children.Contains("uVelocity")) children["uVelocity"] = shader;
            if (effect.Children.Contains("uSource") && !children.Contains("uSource")) children["uSource"] = shader;
            if (effect.Children.Contains("uTarget") && !children.Contains("uTarget")) children["uTarget"] = shader;
            if (effect.Children.Contains("uPressure") && !children.Contains("uPressure")) children["uPressure"] = shader;
        }

        using var shaderEffect = effect.ToShader(true, uniforms, children);
        paint.Shader = shaderEffect;

        target.Canvas.DrawRect(0, 0, width, height, paint);
    }

    private void EnsureSurfaces(GRContext? context)
    {
        _velocity.EnsureInitialized(context);
        _density.EnsureInitialized(context);
        _pressure.EnsureInitialized(context);

        if (_divergence == null || _divergence.Context != context)
        {
            _divergence?.Dispose();
            var info = new SKImageInfo(_simWidth, _simHeight, SKColorType.RgbaF16, SKAlphaType.Premul);
            _divergence = context != null ? SKSurface.Create(context, true, info) : SKSurface.Create(info);
        }

        if (_curl == null || _curl.Context != context)
        {
            _curl?.Dispose();
            var info = new SKImageInfo(_simWidth, _simHeight, SKColorType.RgbaF16, SKAlphaType.Premul);
            _curl = context != null ? SKSurface.Create(context, true, info) : SKSurface.Create(info);
        }
    }

    public void Dispose()
    {
        _velocity.Dispose();
        _density.Dispose();
        _pressure.Dispose();
        _divergence?.Dispose();
        _curl?.Dispose();
    }

    private class CompositeDisposable : IDisposable
    {
        private List<IDisposable> _disposables = new();
        public void Add(IDisposable d) => _disposables.Add(d);
        public void Dispose() { foreach (var d in _disposables) d.Dispose(); }
    }
}
