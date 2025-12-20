namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GenerateFluentExtensionsForAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Scan the namespace of the provided type
    /// </summary>
    public bool ScanNamespace { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetType"></param>
    /// <param name="scanNamespace"></param>
    public GenerateFluentExtensionsForAttribute(Type targetType, bool scanNamespace = false)
    {
        TargetType = targetType;
        ScanNamespace = scanNamespace;
    }
}