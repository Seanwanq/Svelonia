namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ParameterAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public ParameterAttribute(string? name = null)
    {
        Name = name;
    }
}