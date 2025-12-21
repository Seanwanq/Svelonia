using System.IO.Compression;
using Avalonia.Media;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// A subset of the standard Tailwind CSS Color Palette.
/// </summary>
public static class Tw
{
    // Slate
    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate50 = Brush.Parse("#f8fafc");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate100 = Brush.Parse("#f1f5f9");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate200 = Brush.Parse("#e2e8f0");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate300 = Brush.Parse("#cbd5e1");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate400 = Brush.Parse("#94a3b8");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate500 = Brush.Parse("#64748b");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate600 = Brush.Parse("#475569");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate700 = Brush.Parse("#334155");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate800 = Brush.Parse("#1e293b");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Slate900 = Brush.Parse("#0f172a");

    // Red
    /// <summary>
    /// 
    /// </summary>
    public static IBrush Red500 = Brush.Parse("#ef4444");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Red600 = Brush.Parse("#dc2626");

    // Blue
    /// <summary>
    /// 
    /// </summary>
    public static IBrush Blue500 = Brush.Parse("#3b82f6");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Blue600 = Brush.Parse("#2563eb");

    // Green
    /// <summary>
    /// 
    /// </summary>
    public static IBrush Green500 = Brush.Parse("#22c55e");

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Green600 = Brush.Parse("#16a34a");

    // Amber
    /// <summary>
    /// 
    /// </summary>
    public static IBrush Amber500 = Brush.Parse("#f59e0b");

    // Common
    /// <summary>
    /// 
    /// </summary>
    public static IBrush White = Brushes.White;

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Black = Brushes.Black;

    /// <summary>
    /// 
    /// </summary>
    public static IBrush Transparent = Brushes.Transparent;

    /// <summary>
    /// Creates a DynamicResourceExtension for the given key.
    /// </summary>
    public static Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension Resource(string key) => new(key);
}