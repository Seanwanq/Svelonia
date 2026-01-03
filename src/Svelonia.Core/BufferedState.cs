using System;

namespace Svelonia.Core;

/// <summary>
/// A state that buffers changes and only applies them to the source state when committed.
/// Useful for edit forms and dialogs.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BufferedState<T> : State<T>
{
    private readonly State<T> _source;

    /// <summary>
    /// Creates a new BufferedState linked to a source state.
    /// </summary>
    /// <param name="source"></param>
    public BufferedState(State<T> source) : base(source.Value)
    {
        _source = source;
    }

    /// <summary>
    /// Updates the buffer with the current value of the source state.
    /// </summary>
    public void Reset()
    {
        Value = _source.Value;
    }

    /// <summary>
    /// Commits the buffered value back to the source state.
    /// </summary>
    public void Commit()
    {
        _source.Value = Value;
    }
}

public static class BufferedStateExtensions
{
    /// <summary>
    /// Creates a BufferedState wrapper for this state.
    /// </summary>
    public static BufferedState<T> ToBuffered<T>(this State<T> state)
    {
        return new BufferedState<T>(state);
    }
}
