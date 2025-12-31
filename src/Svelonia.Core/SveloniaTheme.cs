using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;

namespace Svelonia.Core;

/// <summary>
/// Simple theme manager for Svelonia
/// </summary>
public class SveloniaTheme
{
    /// <summary>
    /// Load theme from JSON string and apply to Application resources
    /// </summary>
    public static void Load(string json)
    {
        try
        {
            var dict = JsonSerializer.Deserialize(json, SveloniaThemeJsonContext.Default.DictionaryStringObject);
            if (dict == null) return;

            foreach (var item in dict)
            {
                ApplyResource(item.Key, item.Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SveloniaTheme] Error loading theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Load theme from file
    /// </summary>
    public static void LoadFromFile(string path)
    {
        if (File.Exists(path))
        {
            Load(File.ReadAllText(path));
        }
    }

    /// <summary>
    /// Switch between two theme files based on a condition
    /// </summary>
    public static void Switch(string lightPath, string darkPath, bool isDark)
    {
        Apply(isDark ? darkPath : lightPath, isDark ? Avalonia.Styling.ThemeVariant.Dark : Avalonia.Styling.ThemeVariant.Light);
    }

    /// <summary>
    /// Apply a theme from file and optionally set the theme variant
    /// </summary>
    public static void Apply(string path, Avalonia.Styling.ThemeVariant? variant = null)
    {
        LoadFromFile(path);
        if (variant != null && Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = variant;
        }
    }

    /// <summary>
    /// Automatically manages theme files based on the application's actual theme variant.
    /// </summary>
    /// <param name="app">The application instance</param>
    /// <param name="lightThemeState">State containing the theme name/path for Light mode</param>
    /// <param name="darkThemeState">State containing the theme name/path for Dark mode</param>
    /// <param name="basePath">Optional folder path where theme files are located</param>
    public static void Setup(Application app, State<string> lightThemeState, State<string> darkThemeState, string? basePath = null)
    {
        void Update()
        {
            var isDark = app.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
            var themeName = isDark ? darkThemeState.Value : lightThemeState.Value;
            
            if (!themeName.EndsWith(".json")) themeName += ".json";
            
            var finalPath = string.IsNullOrEmpty(basePath) 
                ? themeName 
                : Path.Combine(basePath, themeName);

            LoadFromFile(finalPath);
        }

        app.ActualThemeVariantChanged += (s, e) => Update();
        lightThemeState.OnChange += _ => Update();
        darkThemeState.OnChange += _ => Update();
        
        Update();
    }

    /// <summary>
    /// Retrieves a strongly-typed resource from the application resources.
    /// Throws KeyNotFoundException if missing or invalid type.
    /// </summary>
    public static T Get<T>(string key)
    {
        if (Application.Current != null && Application.Current.TryGetResource(key, null, out var res))
        {
            if (res is T t) return t;
            throw new InvalidCastException($"Resource '{key}' is of type {res?.GetType().Name}, expected {typeof(T).Name}");
        }
        throw new KeyNotFoundException($"Resource '{key}' not found in Application.Resources");
    }

    /// <summary>
    /// Clears all resources that were likely loaded by SveloniaTheme (optional, might be hard to track)
    /// Actually, just overwriting is usually enough if we know the keys.
    /// </summary>
    public static void Clear()
    {
        // No-op for now as we overwrite by key, 
        // but could be used to reset to a clean state if we tracked loaded keys.
    }

    private static void ApplyResource(string key, object value)
    {
        if (Application.Current == null) return;

        object finalValue = value;

        // Try to parse color or gradient if it's a string
        if (value is JsonElement element && element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString();
            if (str == null)
            {
                finalValue = string.Empty;
            }
            else if (str.StartsWith("#") && Color.TryParse(str, out var color))
            {
                finalValue = new SolidColorBrush(color);
            }
            else if (str.StartsWith("linear-gradient("))
            {
                finalValue = ParseLinearGradient(str);
            }
            else if (key.Contains("Shadow") && Color.TryParse(str.Split(' ').Last(), out _))
            {
                finalValue = ParseBoxShadow(str);
            }
            else
            {
                finalValue = str;
            }
        }

        // Use indexer to ensure it updates if already exists
        Application.Current.Resources[key] = finalValue;
    }

    private static BoxShadows ParseBoxShadow(string str)
    {
        // Format: "OffsetX OffsetY Blur [Spread] #Color"
        try
        {
            var parts = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var colorStr = parts.Last();
            if (!Color.TryParse(colorStr, out var color)) color = Colors.Black;

            double offsetX = double.Parse(parts[0]);
            double offsetY = double.Parse(parts[1]);
            double blur = double.Parse(parts[2]);
            double spread = 0;
            if (parts.Count > 4) spread = double.Parse(parts[3]);

            return new BoxShadows(new BoxShadow
            {
                OffsetX = offsetX,
                OffsetY = offsetY,
                Blur = blur,
                Spread = spread,
                Color = color
            });
        }
        catch
        {
            return new BoxShadows(new BoxShadow());
        }
    }

    private static IBrush ParseLinearGradient(string css)
    {
        // Simple parser for linear-gradient(#color1, #color2)
        try
        {
            var content = css.Replace("linear-gradient(", "").Trim().TrimEnd(')');
            var parts = content.Split(',').Select(p => p.Trim()).ToList();
            
            var gradient = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative)
            };

            if (parts.Count == 1) return new SolidColorBrush(Color.Parse(parts[0]));

            for (int i = 0; i < parts.Count; i++)
            {
                if (Color.TryParse(parts[i], out var color))
                {
                    gradient.GradientStops.Add(new GradientStop(color, (double)i / (parts.Count - 1)));
                }
            }

            return gradient;
        }
        catch
        {
            return Brushes.Magenta;
        }
    }
}

[JsonSerializable(typeof(Dictionary<string, object>))]
internal partial class SveloniaThemeJsonContext : JsonSerializerContext
{
}
