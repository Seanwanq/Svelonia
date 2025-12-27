using System.Text.Json;
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
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
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

    private static void ApplyResource(string key, object value)
    {
        if (Application.Current == null) return;

        object finalValue = value;

        // Try to parse color if it's a string starting with #
        if (value is JsonElement element && element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString();
            if (str != null && str.StartsWith("#") && Color.TryParse(str, out var color))
            {
                finalValue = new SolidColorBrush(color);
            }
            else
            {
                finalValue = str ?? string.Empty;
            }
        }
        else if (value is JsonElement num && num.ValueKind == JsonValueKind.Number)
        {
            var val = num.GetDouble();
            // Intelligent conversion for common UI types
            if (key.Contains("Radius", StringComparison.OrdinalIgnoreCase))
            {
                finalValue = new CornerRadius(val);
            }
            else if (key.Contains("Padding", StringComparison.OrdinalIgnoreCase) || 
                     key.Contains("Margin", StringComparison.OrdinalIgnoreCase) ||
                     key.Contains("Thickness", StringComparison.OrdinalIgnoreCase))
            {
                finalValue = new Thickness(val);
            }
            else
            {
                finalValue = val;
            }
        }

        Application.Current.Resources[key] = finalValue;
    }
}
