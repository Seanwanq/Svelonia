using Avalonia.Controls;

namespace Svelonia.Core.Bindings;

/// <summary>
/// 
/// </summary>
public static class ClassExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="classNames"></param>
    /// <returns></returns>
    public static T Classes<T>(this T control, params string[] classNames) where T : Control
    {
        foreach (var name in classNames)
        {
            if (!string.IsNullOrEmpty(name) && !control.Classes.Contains(name))
            {
                control.Classes.Add(name);
            }
        }
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public static T Class<T>(this T control, string className) where T : Control
    {
        control.Classes.Add(className);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="className"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static T Class<T>(this T control, string className, State<bool> condition) where T : Control
    {
        void Update(bool isActive)
        {
            if (isActive)
            {
                if (!control.Classes.Contains(className))
                    control.Classes.Add(className);
            }
            else
            {
                if (control.Classes.Contains(className))
                    control.Classes.Remove(className);
            }
        }

        // Initial
        Update(condition.Value);

        // Lifecycle
        control.AttachedToVisualTree += (s, e) =>
        {
            Update(condition.Value);
            condition.OnChange += Update;
        };

        control.DetachedFromVisualTree += (s, e) =>
        {
            condition.OnChange -= Update;
        };

        if (control.IsLoaded)
        {
            condition.OnChange += Update;
        }

        return control;
    }
}
