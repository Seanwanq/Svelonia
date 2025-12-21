using Avalonia;
using Svelonia.Core;

namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public enum ScreenType
{
    /// <summary>
    /// 
    /// </summary>
    Mobile,

    /// <summary>
    /// 
    /// </summary>
    Tablet,

    /// <summary>
    /// 
    /// </summary>
    Desktop,

    /// <summary>
    /// 
    /// </summary>
    Large,

    /// <summary>
    /// 
    /// </summary>
    ExtraLarge
}

/// <summary>
/// 
/// </summary>
public enum DeviceOrientation
{
    /// <summary>
    /// Width < Height
    /// </summary>
    Portrait,

    /// <summary>
    /// Width >= Height
    /// </summary>
    Landscape
}

/// <summary>
/// 
/// </summary>
public static class MediaQuery
{
    /// <summary>
    /// Configuration
    /// </summary>
    public static class Thresholds
    {
        /// <summary>
        /// 
        /// </summary>
        public static double Mobile { get; set; } = 640;

        /// <summary>
        /// 
        /// </summary>
        public static double Tablet { get; set; } = 768;

        /// <summary>
        /// 
        /// </summary>
        public static double Desktop { get; set; } = 1024;

        /// <summary>
        /// 
        /// </summary>
        public static double Large { get; set; } = 1280;

        /// <summary>
        /// 
        /// </summary>
        public static double ExtraLarge { get; set; } = 1536;
    }

    /// <summary>
    /// Basic state: window width
    /// </summary>
    public static State<double> Width { get; } = new(0);

    /// <summary>
    /// Basic state: window height
    /// </summary>
    public static State<double> Height { get; } = new(0);

    /// <summary>
    /// Current Orientation
    /// </summary>
    public static Computed<DeviceOrientation> Orientation { get; } = new(() =>
        Width.Value >= Height.Value ? DeviceOrientation.Landscape : DeviceOrientation.Portrait);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<ScreenType> ScreenTypeState { get; } = new(() =>
    {
        var w = Width.Value;
        if (w < Thresholds.Mobile) return ScreenType.Mobile;
        if (w < Thresholds.Tablet) return ScreenType.Tablet;
        if (w < Thresholds.Desktop) return ScreenType.Desktop;
        if (w < Thresholds.Large) return ScreenType.Large;
        return ScreenType.ExtraLarge;
    });

    /// <summary>
    /// Common bool breakpoints
    /// </summary>
    public static Computed<bool> IsMobile => new(() => ScreenTypeState.Value == ScreenType.Mobile);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<bool> IsTablet => new(() => ScreenTypeState.Value == ScreenType.Tablet);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<bool> IsDesktop => new(() => ScreenTypeState.Value == ScreenType.Desktop);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<bool> IsLarge => new(() => ScreenTypeState.Value == ScreenType.Large);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<bool> IsExtraLarge => new(() => ScreenTypeState.Value == ScreenType.ExtraLarge);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<bool> IsPortrait => new(() => Orientation.Value == DeviceOrientation.Portrait);

    /// <summary>
    /// 
    /// </summary>
    public static Computed<bool> IsLandscape => new(() => Orientation.Value == DeviceOrientation.Landscape);

    /// <summary>
    /// Custom breakpoint factory
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Computed<bool> Custom(Func<double, double, bool> predicate)
        => new(() => predicate(Width.Value, Height.Value));

    /// <summary>
    /// Update window size
    /// </summary>
    /// <param name="size"></param>
    public static void Update(Size size)
    {
        if (Math.Abs(Width.Value - size.Width) > 0.1) Width.Value = size.Width;
        if (Math.Abs(Height.Value - size.Height) > 0.1) Height.Value = size.Height;
    }

    /// <summary>
    /// Quickly configure global thresholds
    /// </summary>
    public static void Configure(
        double? mobile = null,
        double? tablet = default,
        double? desktop = default,
        double? large = default,
        double? extraLarge = default)
    {
        if (mobile.HasValue) Thresholds.Mobile = mobile.Value;
        if (tablet.HasValue) Thresholds.Tablet = tablet.Value;
        if (desktop.HasValue) Thresholds.Desktop = desktop.Value;
        if (large.HasValue) Thresholds.Large = large.Value;
        if (extraLarge.HasValue) Thresholds.ExtraLarge = extraLarge.Value;
    }

    /// <summary>
    /// Responsively choose value according to screen type (using global thresholds)
    /// </summary>
    public static Computed<T> Select<T>(
        T? mobile = default,
        T? tablet = default,
        T? desktop = default,
        T? pc = default,
        T? large = default,
        T? extraLarge = default,
        T? @default = default)
    {
        var desk = pc ?? desktop;
        return new Computed<T>(() =>
        {
            var type = ScreenTypeState.Value;
            var val = type switch
            {
                ScreenType.Mobile => mobile,
                ScreenType.Tablet => tablet ?? mobile,
                ScreenType.Desktop => desk ?? tablet ?? mobile,
                ScreenType.Large => large ?? desk ?? tablet ?? mobile,
                ScreenType.ExtraLarge => extraLarge ?? large ?? desk ?? tablet ?? mobile,
                _ => @default
            };
            return val ?? @default ?? (default(T)!);
        });
    }

    /// <summary>
    /// Select value based on current orientation.
    /// </summary>
    public static Computed<T> OnOrientation<T>(T portrait, T landscape)
    {
        return new Computed<T>(() => Orientation.Value == DeviceOrientation.Portrait ? portrait : landscape);
    }

    /// <summary>
    /// Select value based on arbitrary custom width breakpoints.
    /// Each breakpoint is (minWidth, value).
    /// </summary>
    public static Computed<T> SelectWidth<T>(params (double minWidth, T value)[] breakpoints)
    {
        return new Computed<T>(() =>
        {
            var w = Width.Value;
            var match = breakpoints
                .OrderByDescending(b => b.minWidth)
                .FirstOrDefault(b => w >= b.minWidth);
            
            return match.value;
        });
    }

    /// <summary>
    /// Select value based on arbitrary custom height breakpoints.
    /// Each breakpoint is (minHeight, value).
    /// </summary>
    public static Computed<T> SelectHeight<T>(params (double minHeight, T value)[] breakpoints)
    {
        return new Computed<T>(() =>
        {
            var h = Height.Value;
            var match = breakpoints
                .OrderByDescending(b => b.minHeight)
                .FirstOrDefault(b => h >= b.minHeight);
            
            return match.value;
        });
    }
}

/// <summary>
/// Helper for platform-specific selections
/// </summary>
public static class Platform
{
    /// <summary>
    /// 
    /// </summary>
    public static bool IsWindows => OperatingSystem.IsWindows();

    /// <summary>
    /// 
    /// </summary>
    public static bool IsMacOS => OperatingSystem.IsMacOS();

    /// <summary>
    /// 
    /// </summary>
    public static bool IsLinux => OperatingSystem.IsLinux();

    /// <summary>
    /// 
    /// </summary>
    public static bool IsAndroid => OperatingSystem.IsAndroid();

    /// <summary>
    /// 
    /// </summary>
    public static bool IsIOS => OperatingSystem.IsIOS();

    /// <summary>
    /// 
    /// </summary>
    public static bool IsBrowser => OperatingSystem.IsBrowser();

    /// <summary>
    /// Select value based on current platform
    /// </summary>
    public static T Select<T>(
        T? windows = default,
        T? macos = default,
        T? linux = default,
        T? android = default,
        T? ios = default,
        T? browser = default,
        T? @default = default)
    {
        if (IsWindows && windows != null) return windows;
        if (IsMacOS && macos != null) return macos;
        if (IsLinux && linux != null) return linux;
        if (IsAndroid && android != null) return android;
        if (IsIOS && ios != null) return ios;
        if (IsBrowser && browser != null) return browser;
        return @default!;
    }
}