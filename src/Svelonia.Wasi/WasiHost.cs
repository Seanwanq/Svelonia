using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Svelonia.Core;
using Wasmtime;

#pragma warning disable IL3050 // AOT JSON warning
#pragma warning disable IL2026 // Trimming warning

namespace Svelonia.Wasi;

public interface IWasiPlugin : IDisposable
{
    string Id { get; }
    void Initialize(WasiHost host);
    string? Call(string functionName, params object[] args);
}

public class WasiHost : IDisposable
{
    private readonly Engine _engine;
    private readonly Linker _linker;
    private readonly Store _store;
    private readonly WasiConfiguration _wasiConfig;
    private readonly Dictionary<string, IState> _states = new();
    private readonly Dictionary<string, List<IWasiPlugin>> _subscribers = new();
    private IWasiPlugin? _currentCallingPlugin;

    private readonly List<IWasiExtension> _extensions = new();
    private readonly Dictionary<string, HashSet<string>> _pluginPermissions = new();

    public WasiHost(WasiHostConfiguration? config = null)
    {
        config ??= new WasiHostConfiguration();

        var wConfig = new Config();
        // if (config.MaxFuel.HasValue)
        // {
        //     wConfig.WithFuelConsumption(true);
        // }

        _engine = new Engine(wConfig);
        _linker = new Linker(_engine);
        _store = new Store(_engine);

        if (config.MaxFuel.HasValue)
        {
            // _store.AddFuel((ulong)config.MaxFuel.Value); // API mismatch in Wasmtime 34.0.2?
        }

        _wasiConfig = new WasiConfiguration();

        _linker.DefineWasi();
        _store.SetWasiConfiguration(_wasiConfig);

        // Register Core Extension by default
        RegisterExtension(new CoreExtension(this));
    }

    public bool CheckPermission(string permission)
    {
        if (_currentCallingPlugin == null) return true; // Host calling itself is trusted

        if (_pluginPermissions.TryGetValue(_currentCallingPlugin.Id, out var perms))
        {
            return perms.Contains(permission);
        }
        return false; // Default safe: if no permissions entry, deny? Or allow if null? 
        // WasiHost code below initializes entry if explicit permissions passed.
        // If loaded without permissions, assume none?
        // Let's assume strict default deny if entry exists, but if entry missing... 
        // Actually LoadPlugin adds entry.
        return false;
    }

    public void RegisterExtension(IWasiExtension extension)
    {
        _extensions.Add(extension);
        extension.Register(_linker, _store);
    }

    public T? GetExtension<T>() where T : class, IWasiExtension
    {
        return _extensions.OfType<T>().FirstOrDefault();
    }

    // Public API for extensions to access internal state (Consider making internal friends or exposing cleaner API)
    public IState? GetState(string name) => _states.TryGetValue(name, out var s) ? s : null;

    public void Subscribe(string name)
    {
        if (_currentCallingPlugin != null)
        {
            if (!_subscribers.ContainsKey(name)) _subscribers[name] = new();
            if (!_subscribers[name].Contains(_currentCallingPlugin))
                _subscribers[name].Add(_currentCallingPlugin);
        }
    }

    public void RegisterState(string name, IState state)
    {
        _states[name] = state;

        state.OnChangeObject += (val) =>
        {
            if (_subscribers.TryGetValue(name, out var plugins))
            {
                var json = JsonSerializer.Serialize(val);
                foreach (var plugin in plugins)
                {
                    plugin.Call("on_state_changed", name, json);
                }
            }
        };
    }

    public IWasiPlugin LoadPlugin(string wasmPath, IEnumerable<string>? permissions = null)
    {
        var module = Module.FromFile(_engine, wasmPath);
        var plugin = new WasiPlugin(this, module, _store, _linker);

        if (permissions != null)
        {
            _pluginPermissions[plugin.Id] = new HashSet<string>(permissions);
        }

        return plugin;
    }


    public void Dispose()
    {
        _store.Dispose();
        _engine.Dispose();
    }

    private class WasiPlugin : IWasiPlugin
    {
        private readonly WasiHost _host;
        private readonly Instance _instance;
        private readonly Memory? _memory;
        private readonly Function? _alloc;
        private readonly Function? _free;
        public string Id { get; } = Guid.NewGuid().ToString();

        public WasiPlugin(WasiHost host, Module module, Store store, Linker linker)
        {
            _host = host;
            _instance = linker.Instantiate(store, module);
            _memory = _instance.GetMemory("memory");
            _alloc = _instance.GetFunction("svelonia_alloc");
            _free = _instance.GetFunction("svelonia_free");
        }

        public void Initialize(WasiHost host)
        {
            _host._currentCallingPlugin = this;
            try
            {
                var init = _instance.GetFunction("initialize");
                init?.Invoke();
            }
            finally
            {
                _host._currentCallingPlugin = null;
            }
        }

        public string? Call(string functionName, params object[] args)
        {
            _host._currentCallingPlugin = this;
            var ptrsToFree = new List<int>();
            try
            {
                var func = _instance.GetFunction(functionName);
                if (func == null)
                    return null;

                var processedArgs = new List<ValueBox>();
                foreach (var arg in args)
                {
                    if (arg is string s)
                    {
                        var ptr = PassString(s);
                        ptrsToFree.Add(ptr);
                        processedArgs.Add(ptr);
                        processedArgs.Add(Encoding.UTF8.GetByteCount(s));
                    }
                    else if (arg is int i)
                        processedArgs.Add(i);
                    else if (arg is float f)
                        processedArgs.Add(f);
                    else if (arg is double d)
                        processedArgs.Add(d);
                    else if (arg is bool b)
                        processedArgs.Add(b ? 1 : 0);
                    else if (arg is ValueBox vb)
                        processedArgs.Add(vb);
                }

                var result = func.Invoke(processedArgs.ToArray());
                return result?.ToString();
            }
            finally
            {
                // Free allocated strings
                foreach (var ptr in ptrsToFree)
                {
                    _free?.Invoke(ptr);
                }
                _host._currentCallingPlugin = null;
            }
        }

        private int PassString(string s)
        {
            if (_memory == null || _alloc == null)
                return 0;

            int len = Encoding.UTF8.GetByteCount(s);
            int ptr = (int)_alloc.Invoke(len)!;
            _memory.WriteString(ptr, s);
            return ptr;
        }

        public void Dispose() { }
    }
}

public enum DrawOp
{
    BeginPath,
    AddPoint,
    EndPath,
}

public record struct DrawCommand(
    DrawOp Op,
    string Id,
    double X,
    double Y,
    string? Color = null,
    double Thickness = 1.0
);
