using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelonia.Wasi.Sdk;

public static class SvePlugin
{
    // --- Host Imports ---

    [DllImport("svelonia", EntryPoint = "log")]
    private static extern unsafe void HostLog(byte* ptr, int len);

    [DllImport("svelonia", EntryPoint = "get_state")]
    private static extern unsafe void HostGetState(
        byte* namePtr,
        int nameLen,
        byte* resultPtr,
        int maxLen
    );

    [DllImport("svelonia", EntryPoint = "set_state")]
    private static extern unsafe void HostSetState(
        byte* namePtr,
        int nameLen,
        byte* valPtr,
        int valLen
    );

    [DllImport("svelonia", EntryPoint = "subscribe")]
    private static extern unsafe void HostSubscribe(byte* namePtr, int nameLen);

    // --- Helper Methods ---

    public static unsafe void Log(string message)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(message);
        fixed (byte* ptr = bytes)
        {
            HostLog(ptr, bytes.Length);
        }
    }

    public static unsafe void Subscribe(string stateName)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(stateName);
        fixed (byte* ptr = bytes)
        {
            HostSubscribe(ptr, bytes.Length);
        }
    }

    public static unsafe void SetState(string name, string jsonValue)
    {
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
        var valBytes = System.Text.Encoding.UTF8.GetBytes(jsonValue);
        fixed (byte* nPtr = nameBytes)
        fixed (byte* vPtr = valBytes)
        {
            HostSetState(nPtr, nameBytes.Length, vPtr, valBytes.Length);
        }
    }

    // --- Plugin Events ---

    public static event Action? OnInitialize;
    public static event Action<string, string>? OnStateChanged;

    // --- Wasm Exports ---

    [UnmanagedCallersOnly(EntryPoint = "svelonia_alloc")]
    public static unsafe IntPtr _Alloc(int len)
    {
        // Simple allocation for the bridge
        return Marshal.AllocHGlobal(len);
    }

    [UnmanagedCallersOnly(EntryPoint = "initialize")]
    public static void _Initialize() => OnInitialize?.Invoke();

    [UnmanagedCallersOnly(EntryPoint = "on_state_changed")]
    public static unsafe void _OnStateChanged(byte* namePtr, int nameLen, byte* valPtr, int valLen)
    {
        var name = System.Text.Encoding.UTF8.GetString(namePtr, nameLen);
        var value = System.Text.Encoding.UTF8.GetString(valPtr, valLen);
        OnStateChanged?.Invoke(name, value);
    }
}
