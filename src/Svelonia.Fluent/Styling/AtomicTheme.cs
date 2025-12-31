using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// Provides atomic design theme settings
/// </summary>
public static class AtomicTheme
{
    /// <summary>
    /// The base unit size for spacing (Padding/Margin). Default is 1.0.
    /// Example: .P(4) means 4 * 1.0 = 4px.
    /// </summary>
    public static double UnitSize { get; set; } = 1.0;

    /// <summary>
    /// Helper to calculate actual pixels.
    /// </summary>
    public static double GetSize(double units) => units * UnitSize;
}