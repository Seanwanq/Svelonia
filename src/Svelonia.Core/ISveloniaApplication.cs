using System;

namespace Svelonia.Core;

/// <summary>
/// Represents a Svelonia application that provides access to a ServiceProvider.
/// Implementing this interface enables framework extensions (like keyboard bindings)
/// to resolve services (like Mediator) in an AOT-safe manner.
/// </summary>
public interface ISveloniaApplication
{
    /// <summary>
    /// Gets the application's service provider.
    /// </summary>
    IServiceProvider? Services { get; }
}
