using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Svelonia.Gen;

[Generator]
public class WasiBinderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsTarget(s),
                transform: static (ctx, _) => GetTarget(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsTarget(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;
    }

    private static ClassDeclarationSyntax? GetTarget(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeSymbol &&
                    attributeSymbol.ContainingType.ToDisplayString() == "Svelonia.Wasi.WasiModuleAttribute")
                {
                    return classDeclaration;
                }
            }
        }
        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        foreach (var classDecl in classes.Distinct())
        {
            if (classDecl == null) continue;

            var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol) continue;

            var moduleAttr = classSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Svelonia.Wasi.WasiModuleAttribute");

            if (moduleAttr == null || moduleAttr.ConstructorArguments.Length == 0) continue;

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;
            string wasiModule = moduleAttr.ConstructorArguments[0].Value?.ToString() ?? "env";

            var sb = new StringBuilder();
            sb.AppendLine("using Svelonia.Wasi;");
            sb.AppendLine("using Wasmtime;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Text.Json;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        public void RegisterGenerated(Linker linker, Store store)");
            sb.AppendLine("        {");
            sb.AppendLine("            var binder = new WasiBinder(store);");

            foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                var funcAttr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Svelonia.Wasi.WasiFunctionAttribute");

                if (funcAttr == null) continue;

                string wasiFunc = funcAttr.ConstructorArguments[0].Value?.ToString() ?? member.Name;
                string methodName = member.Name;

                // Analyze parameters
                var parameters = member.Parameters;
                var returnType = member.ReturnType;
                bool isVoid = returnType.SpecialType == SpecialType.System_Void;

                // Generator Logic:
                // We map known patterns to binder.Define calls.
                // 1. Action<string>
                // 2. Action<string, string>
                // 3. Action<string, double>
                // 4. Action<string, string, double> (DrawBegin)
                // 5. Func<string, string> (GetState)
                // 6. Func<string, T> (Now) -> complex return

                sb.AppendLine();
                sb.AppendLine($"            // Binding {methodName} -> {wasiFunc}");

                if (isVoid)
                {
                    if (parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String)
                    {
                        // Action<string>
                        sb.AppendLine($"            binder.DefineAction<string>(linker, \"{wasiModule}\", \"{wasiFunc}\", this.{methodName});");
                    }
                    else if (parameters.Length == 2 &&
                             parameters[0].Type.SpecialType == SpecialType.System_String &&
                             parameters[1].Type.SpecialType == SpecialType.System_String)
                    {
                        // Action<string, string>
                        sb.AppendLine($"            binder.DefineAction<string, string>(linker, \"{wasiModule}\", \"{wasiFunc}\", this.{methodName});");
                    }
                    else if (parameters.Length == 3 &&
                             parameters[0].Type.SpecialType == SpecialType.System_String &&
                             parameters[1].Type.SpecialType == SpecialType.System_String &&
                             parameters[2].Type.SpecialType == SpecialType.System_Double)
                    {
                        // DrawBegin special case
                        sb.AppendLine($"            binder.DefineDrawBegin(linker, \"{wasiModule}\", \"{wasiFunc}\", this.{methodName});");
                    }
                    else if (parameters.Length == 3 &&
                            parameters[0].Type.SpecialType == SpecialType.System_String &&
                            parameters[1].Type.SpecialType == SpecialType.System_Double &&
                            parameters[2].Type.SpecialType == SpecialType.System_Double)
                    {
                        // DrawAddPoint
                        sb.AppendLine($"            // Manual emit for DrawAddPoint signature");
                        sb.AppendLine($"            linker.Define(\"{wasiModule}\", \"{wasiFunc}\", Function.FromCallback(store, (Caller caller, int p1, int l1, double x, double y) => {{");
                        sb.AppendLine($"                var mem = caller.GetMemory(\"memory\");");
                        sb.AppendLine($"                var id = mem?.ReadString(p1, l1);");
                        sb.AppendLine($"                if (id != null) this.{methodName}(id, x, y);");
                        sb.AppendLine($"            }}));");
                    }
                }
                else
                {
                    // Func<T, R>
                    if (parameters.Length == 1 && parameters[0].Type.SpecialType == SpecialType.System_String)
                    {
                        var returnTypeStr = returnType.ToDisplayString();
                        // Func<string, R>
                        // If R is string, use DefineFunc<string, string>
                        // If R is complex, use DefineFunc<string, Complex>
                        sb.AppendLine($"            binder.DefineFunc<string, {returnTypeStr}>(linker, \"{wasiModule}\", \"{wasiFunc}\", this.{methodName});");
                    }
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"{className}.g.cs", sb.ToString());
        }
    }
}
