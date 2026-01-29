using System.Linq;
using System.Reflection;
using Wasmtime;

#pragma warning disable IL3050 // AOT Reflection warning
#pragma warning disable IL2026 // Trimming warning
#pragma warning disable IL2060 // MakeGenericMethod warning

namespace Svelonia.Wasi;

public class WasiAutoBinder
{
    private readonly WasiBinder _binder;
    private readonly Linker _linker;
    private readonly Store _store;

    public WasiAutoBinder(Linker linker, Store store)
    {
        _linker = linker;
        _store = store;
        _binder = new WasiBinder(store);
    }

    public void BindExtension(object extension)
    {
        var type = extension.GetType();
        var moduleAttr = type.GetCustomAttribute<WasiModuleAttribute>();
        if (moduleAttr == null) return; // Not a declarative module

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          .Where(m => m.GetCustomAttribute<WasiFunctionAttribute>() != null);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<WasiFunctionAttribute>()!;
            BindMethod(moduleAttr.Namespace, attr.Name, method, extension);
        }
    }

    private void BindMethod(string module, string name, MethodInfo method, object target)
    {
        var paramsInfo = method.GetParameters();
        var returnType = method.ReturnType;

        // Simplify for Demo: Supporting Action<T> and Action<T1, T2> and Func<T, R>
        // In a full implementation, we would construct specific delegates or use expression trees to wrap 'target'.

        if (returnType == typeof(void))
        {
            if (paramsInfo.Length == 1)
            {
                // Action<T1>
                var t1 = paramsInfo[0].ParameterType;
                var bindMethod = typeof(WasiBinder).GetMethod("DefineAction", new[] { typeof(Linker), typeof(string), typeof(string), typeof(Action<>).MakeGenericType(t1) });

                // Create Delegate: Action<T1>
                var delType = typeof(Action<>).MakeGenericType(t1);
                var del = method.CreateDelegate(delType, target);

                bindMethod?.MakeGenericMethod(t1).Invoke(_binder, new object[] { _linker, module, name, del });
            }
            else if (paramsInfo.Length == 2)
            {
                // Action<T1, T2>
                var t1 = paramsInfo[0].ParameterType;
                var t2 = paramsInfo[1].ParameterType;
                var bindMethod = typeof(WasiBinder).GetMethod("DefineAction", 2, new[] { typeof(Linker), typeof(string), typeof(string), typeof(Action<,>).MakeGenericType(t1, t2) });

                var delType = typeof(Action<,>).MakeGenericType(t1, t2);
                var del = method.CreateDelegate(delType, target);

                bindMethod?.MakeGenericMethod(t1, t2).Invoke(_binder, new object[] { _linker, module, name, del });
            }
            else if (paramsInfo.Length == 3 && name == "draw_begin_path")
            {
                // Special case for draw_begin_path or generic 3 args
                // For now, let's hardcode the delegate creation for the 3-arg DrawBegin
                var del = (Action<string, string, double>)method.CreateDelegate(typeof(Action<string, string, double>), target);
                _binder.DefineDrawBegin(_linker, module, name, del);
            }
        }
        else
        {
            if (paramsInfo.Length == 1)
            {
                // Func<T1, TResult>
                var t1 = paramsInfo[0].ParameterType;
                var tRes = returnType;
                var bindMethod = typeof(WasiBinder).GetMethod("DefineFunc", new[] { typeof(Linker), typeof(string), typeof(string), typeof(Func<,>).MakeGenericType(t1, tRes) });

                var delType = typeof(Func<,>).MakeGenericType(t1, tRes);
                var del = method.CreateDelegate(delType, target);

                bindMethod?.MakeGenericMethod(t1, tRes).Invoke(_binder, new object[] { _linker, module, name, del });
            }
        }
    }
}
