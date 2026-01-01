namespace Svelonia.Fluent;

/// <summary>
/// Semantic Design System Constants (Global Presets).
/// Provides standardized values for spacing, sizing, and corner radius.
/// Usage: .P(G.Small) or .Rounded(G.RadiusMedium)
/// </summary>
public static class G
{
    // Spacing (Gap/Margin/Padding)
    public static double None { get; set; } = 0;
    public static double Tiny { get; set; } = 4.0;
    public static double Small { get; set; } = 8.0;
    public static double Medium { get; set; } = 16.0;
    public static double Large { get; set; } = 24.0;
    public static double XLarge { get; set; } = 32.0;
    public static double Huge { get; set; } = 48.0;
    public static double Massive { get; set; } = 64.0;

    // Layout Sizes
    public static double ControlHeight { get; set; } = 32.0;
    public static double ButtonHeight { get; set; } = 32.0;
    public static double InputHeight { get; set; } = 32.0;
    public static double IconSize { get; set; } = 16.0;
    public static double IconSizeLarge { get; set; } = 24.0;

    // Corner Radius
    public static double RadiusSmall { get; set; } = 4.0;
    public static double RadiusMedium { get; set; } = 8.0;
    public static double RadiusLarge { get; set; } = 12.0;
    public static double RadiusFull { get; set; } = 9999.0;

    // Opacity
    public static double DisabledOpacity { get; set; } = 0.5;
}
