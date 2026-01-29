using System;

namespace Svelonia.Wasi;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class WasiModuleAttribute : Attribute
{
    public string Namespace { get; }
    public WasiModuleAttribute(string ns) => Namespace = ns;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class WasiFunctionAttribute : Attribute
{
    public string Name { get; }
    public WasiFunctionAttribute(string name) => Name = name;
}
