namespace Svelonia.Core.Responsive;

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
    Desktop
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
        public static double Mobile { get; set; } = 769;

        /// <summary>
        /// 
        /// </summary>
        public static double Tablet { get; set; } = 1024;
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
    /// 
    /// </summary>
    public static Computed<ScreenType> ScreenTypeState { get; } = new(() =>
    {
        var w = Width.Value;
        if (w < Thresholds.Mobile) return ScreenType.Mobile;
        if (w < Thresholds.Tablet) return ScreenType.Tablet;
        return ScreenType.Desktop;
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
    /// Compose breakpoints
    /// </summary>
    public static Computed<bool> IsMobileOrTablet => new(() => Width.Value < Thresholds.Tablet);

    /// <summary>
    /// Custom breakpoint factory
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static Computed<bool> Custom(Func<double, bool> predicate)
        => new(() => predicate(Width.Value));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size"></param>
    public static void Update(Avalonia.Size size)
    {
        if (Math.Abs(Width.Value - size.Width) > 0.1) Width.Value = size.Width;
        if (Math.Abs(Height.Value - size.Height) > 0.1) Height.Value = size.Height;
    }

    /// <summary>
    /// Responsively choose value according to screen type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mobile"></param>
    /// <param name="tablet"></param>
    /// <param name="desktop"></param>
    /// <returns></returns>
    public static Computed<T> Select<T>(T mobile, T tablet, T desktop)
    {
        return new Computed<T>(() =>
        {
            return ScreenTypeState.Value switch
            {
                ScreenType.Mobile => mobile,
                ScreenType.Tablet => tablet,
                ScreenType.Desktop => desktop,
                _ => desktop
            };
        });
    }

    /// <summary>
    /// Choose value according to screen type (Mobile or Others)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mobile"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static Computed<T> Select<T>(T mobile, T other)
    {
        return new Computed<T>(() => IsMobile.Value ? mobile : other);
    }
}