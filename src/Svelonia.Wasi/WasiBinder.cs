using System;
using System.Text;
using Wasmtime;

namespace Svelonia.Wasi;

public class WasiBinder
{
    private readonly Store _store;

    public WasiBinder(Store store)
    {
        _store = store;
    }

    public void DefineAction(Linker linker, string module, string name, Action callback)
    {
        linker.Define(module, name, Function.FromCallback(_store, (Caller c) => callback()));
    }

    public void DefineAction<T1>(Linker linker, string module, string name, Action<T1> callback)
    {
        if (typeof(T1) == typeof(string))
        {
            linker.Define(module, name, Function.FromCallback(_store, (Caller c, int ptr, int len) =>
            {
                var s = ReadString(c, ptr, len);
                ((Action<string>)(object)callback)(s);
            }));
        }
        else
        {
            linker.Define(module, name, Function.FromCallback(_store, (Caller c, T1 arg1) => callback(arg1)));
        }
    }

    public void DefineAction<T1, T2>(Linker linker, string module, string name, Action<T1, T2> callback)
    {
        // Simple heuristic for now: only supporting explicit string positions or primitives
        // A full implementation would use expression trees or reflection to build the wrapper.
        // For this demo, let's hardcode common patterns or use dynamic dispatch?
        // Dynamic dispatch overhead is fine for this prototype.

        // Pattern 1: String, Double (e.g. log with level?)
        // Pattern 2: String, String (set_state key, val) -> (ptr, len, ptr, len)

        // To be truly generic without code gen, we need to inspect types.
        if (typeof(T1) == typeof(string) && typeof(T2) == typeof(string))
        {
            linker.Define(module, name, Function.FromCallback(_store, (Caller c, int p1, int l1, int p2, int l2) =>
           {
               var s1 = ReadString(c, p1, l1);
               var s2 = ReadString(c, p2, l2);
               ((Action<string, string>)(object)callback)(s1, s2);
           }));
        }
        else if (typeof(T1) == typeof(string) && typeof(T2) == typeof(double))
        {
            linker.Define(module, name, Function.FromCallback(_store, (Caller c, int p1, int l1, double arg2) =>
           {
               var s1 = ReadString(c, p1, l1);
               ((Action<string, double>)(object)callback)(s1, arg2);
           }));
        }
        else
        {
            // Fallback for primitives
            linker.Define(module, name, Function.FromCallback(_store, (Caller c, T1 arg1, T2 arg2) => callback(arg1, arg2)));
        }
    }

    // Custom overload for DrawBeginPath (String, String, Double) -> (ptr,len, ptr,len, double)
    public void DefineDrawBegin(Linker linker, string module, string name, Action<string, string, double> callback)
    {
        linker.Define(module, name, Function.FromCallback(_store, (Caller c, int p1, int l1, int p2, int l2, double arg3) =>
        {
            var s1 = ReadString(c, p1, l1);
            var s2 = ReadString(c, p2, l2);
            callback(s1, s2, arg3);
        }));
    }

    public void DefineFunc<T1, TResult>(Linker linker, string module, string name, Func<T1, TResult> callback)
    {
        if (typeof(T1) == typeof(string) && typeof(TResult) == typeof(string))
        {
            linker.Define(module, name, Function.FromCallback(_store, (Caller c, int p1, int l1) =>
           {
               var s1 = ReadString(c, p1, l1);
               var result = ((Func<string, string>)(object)callback)(s1);
               return WriteString(c, result);
           }));
        }
    }

    private string ReadString(Caller c, int ptr, int len)
    {
        var mem = c.GetMemory("memory");
        if (mem == null) return "";
        return mem.ReadString(ptr, len);
    }

    private int WriteString(Caller c, string s)
    {
        var alloc = c.GetFunction("svelonia_alloc");
        if (alloc == null) return 0; // Error: Guest must export allocator

        int len = Encoding.UTF8.GetByteCount(s);
        int ptr = (int)alloc.Invoke(len)!;

        var mem = c.GetMemory("memory");
        mem?.WriteString(ptr, s);
        return ptr;
    }
}
