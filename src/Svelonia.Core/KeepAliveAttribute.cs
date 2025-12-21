namespace Svelonia.Core;

/// <summary>
/// Marks a page or component to be kept alive (cached) in the router.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class KeepAliveAttribute : Attribute
{
}
