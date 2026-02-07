
namespace Svelonia.Physics.Fluid;

/// <summary>
/// Standard SkSL implementations of Eulerian Fluid Dynamics equations.
/// Based on "Target-Driven Smoke Animation" (Fatemi et al.) and GPU Gems Chapter 38 (Fast Fluid Dynamics).
/// </summary>
public static class FluidEquations
{
    // 1. Advection: Moves a quantity (velocity, density) along the velocity field.
    // q(x, t + dt) = q(x - u(x,t)*dt, t)
    public const string Advection = @"
        uniform shader uVelocity;
        uniform shader uSource;
        uniform float uDt;
        uniform float uDissipation;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
            float2 uv = coords * uTexelSize;
            
            // Sample velocity at current position
            float2 vel = sample(uVelocity, coords).xy;
            
            // Backtrace: Find where the particle was 'dt' time ago
            float2 prevPos = uv - vel * uDt * uTexelSize; // Note: uTexelSize scaling depends on coordinate system
            
            // Sample the quantity at the previous position (Bilinear interpolation is automatic in SkShader)
            half4 result = sample(uSource, prevPos / uTexelSize);
            
            // Apply dissipation (decay)
            return result * half(uDissipation);
        }
    ";

    // 2. Ext Forces (Splats): Adds Gaussian impulse.
    public const string Impulse = @"
        uniform shader uTarget;
        uniform float2 uPoint;      // Center in UV (0-1)
        uniform float3 uColor;
        uniform float uRadius;
        uniform float2 uTexelSize;
        uniform float uAspectRatio;

        half4 main(float2 coords) {
            float2 uv = coords * uTexelSize;
            
            // Distance from impulse center
            float2 p = uv - uPoint;
            p.x *= uAspectRatio; // Correct for non-square aspect
            
            // Gaussian Falloff: exp(-dist^2 / radius)
            float splat = exp(-dot(p, p) / uRadius);
            
            half4 baseColor = sample(uTarget, coords);
            half3 impulse = uColor * splat;
            
            return baseColor + half4(impulse, 0.0);
        }
    ";

    // 3. Curl: measures the rotation of the velocity field.
    // curl = dV/dx - dU/dy
    // Used for vorticity confinement to restore swirly details lost to dissipation.
    public const string Curl = @"
        uniform shader uVelocity;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
            // Finite difference samples
            // L, R, T, B
            float2 L = sample(uVelocity, coords - float2(1, 0)).xy;
            float2 R = sample(uVelocity, coords + float2(1, 0)).xy;
            float2 T = sample(uVelocity, coords - float2(0, 1)).xy;
            float2 B = sample(uVelocity, coords + float2(0, 1)).xy;

            // Curl formulation
            float curl = (R.y - L.y) - (B.x - T.x);
            return half4(curl * 0.5, 0, 0, 1);
        }
    ";

    // 4. Vorticity Confinement: amplifies the curl to add small-scale details.
    // F_vc = N x (curl)
    public const string Vorticity = @"
        uniform shader uVelocity;
        uniform shader uCurl;
        uniform float uCurlStrength;
        uniform float uDt;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
            float curl = sample(uCurl, coords).x;
            
            // Gradient of Curl
            float C_L = sample(uCurl, coords - float2(1, 0)).x;
            float C_R = sample(uCurl, coords + float2(1, 0)).x;
            float C_T = sample(uCurl, coords - float2(0, 1)).x;
            float C_B = sample(uCurl, coords + float2(0, 1)).x;
            
            float forceX = abs(C_T) - abs(C_B);
            float forceY = abs(C_R) - abs(C_L);
            
            // Normalize force direction
            float len = sqrt(forceX * forceX + forceY * forceY) + 0.0001; // Avoid div0
            forceX /= len;
            forceY /= len;
            
            float2 force = float2(forceX, forceY) * uCurlStrength * curl;
            
            float2 vel = sample(uVelocity, coords).xy;
            return half4(vel + force * uDt, 0.0, 1.0);
        }
    ";

    // 5. Divergence: Calculates how much "stuff" is entering/leaving a cell.
    // div = dU/dx + dV/dy
    // Code heavily inspired by classic Stam implementation.
    public const string Divergence = @"
        uniform shader uVelocity;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
            float2 L = sample(uVelocity, coords - float2(1, 0)).xy;
            float2 R = sample(uVelocity, coords + float2(1, 0)).xy;
            float2 T = sample(uVelocity, coords - float2(0, 1)).xy;
            float2 B = sample(uVelocity, coords + float2(0, 1)).xy;

            float div = 0.5 * ((R.x - L.x) + (B.y - T.y));
            return half4(div, 0, 0, 1);
        }
    ";

    // 6. Jacobi Iteration (Pressure Solver): Solves for Pressure field.
    // P_new = (P_L + P_R + P_T + P_B - divergence) / 4
    public const string Jacobi = @"
        uniform shader uPressure; // Previous iteration
        uniform shader uDivergence;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
            float L = sample(uPressure, coords - float2(1, 0)).x;
            float R = sample(uPressure, coords + float2(1, 0)).x;
            float T = sample(uPressure, coords - float2(0, 1)).x;
            float B = sample(uPressure, coords + float2(0, 1)).x;
            
            float div = sample(uDivergence, coords).x;
            
            float pNew = (L + R + T + B - div) * 0.25;
            return half4(pNew, 0, 0, 1);
        }
    ";

    // 7. Gradient Subtraction: Project velocity to be mass-conserving (incompressible).
    // U_new = U_old - Gradient(P)
    public const string SubtractGradient = @"
        uniform shader uPressure;
        uniform shader uVelocity;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
             float L = sample(uPressure, coords - float2(1, 0)).x;
             float R = sample(uPressure, coords + float2(1, 0)).x;
             float T = sample(uPressure, coords - float2(0, 1)).x;
             float B = sample(uPressure, coords + float2(0, 1)).x;
             
             float2 vel = sample(uVelocity, coords).xy;
             float2 grad = float2(R - L, B - T) * 0.5;
             
             return half4(vel - grad, 0, 1);
        }
    ";

    // 8. Visualization
    public const string Display = @"
        uniform shader uTexture;
        uniform float2 uTexelSize;

        half4 main(float2 coords) {
            half4 color = sample(uTexture, coords);
            // Optionally could add tone mapping or alpha adjustment here
            return color;
        }
    ";
}
