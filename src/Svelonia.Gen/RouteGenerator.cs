using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Svelonia.Gen;

[Generator]
public class RouteGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax,
                transform: (ctx, _) => GetRoutableClass(ctx))
            .Where(m => m != null);

        context.RegisterSourceOutput(provider.Collect(), Execute);
    }

    private static INamedTypeSymbol? GetRoutableClass(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (symbol is null || symbol.IsAbstract) return null;

        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            var ns = baseType.ContainingNamespace?.ToString();
            if ((baseType.Name == "Page" && ns == "Svelonia.Kit") ||
                (baseType.Name == "Layout" && ns == "Svelonia.Core"))
            {
                return symbol;
            }
            baseType = baseType.BaseType;
        }

        return null;
    }

    private void Execute(SourceProductionContext context, System.Collections.Immutable.ImmutableArray<INamedTypeSymbol?> symbols)
    {
        if (symbols.IsDefaultOrEmpty) return;

        var pages = new List<INamedTypeSymbol>();
        var layouts = new Dictionary<string, INamedTypeSymbol>();

        foreach (var sym in symbols)
        {
            if (sym is null) continue;
            if (InheritsFrom(sym, "Layout"))
            {
                layouts[sym.ContainingNamespace.ToString()] = sym;
            }
            else
            {
                pages.Add(sym);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("using Svelonia.Kit;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("namespace Svelonia.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    internal static class RouteRegistry");
        sb.AppendLine("    {");
        sb.AppendLine("        public static void RegisterRoutes(Router router, IServiceProvider services)");
        sb.AppendLine("        {");

        foreach (var page in pages)
        {
            var route = GenerateRoute(page);

            sb.Append("            router.Register(\"");
            sb.Append(route);
            sb.AppendLine("\", async p => {");

            // 1. Construct Page
            var currentConstruction = GenerateConstruction(page, null);
            sb.AppendLine($"                var page = {currentConstruction};");

            // Parameter Injection
            GenerateParameterInjection(sb, page);

            sb.AppendLine($"                await page.OnLoadAsync(p);");

            // 2. Cascading Layouts (Walk up the namespace)
            // We want innermost layout first (closest to page), then outer layouts.
            // But we construct objects from inside out: new Outer( new Inner ( page ) )
            // So we need to find all layouts, and wrap them in order.

            var collectedLayouts = new List<INamedTypeSymbol>();
            var nsSymbol = page.ContainingNamespace;

            while (nsSymbol != null && !nsSymbol.IsGlobalNamespace)
            {
                var nsName = nsSymbol.ToString();
                if (layouts.TryGetValue(nsName, out var layout))
                {
                    collectedLayouts.Add(layout);
                }
                nsSymbol = nsSymbol.ContainingNamespace;
            }

            // collectedLayouts has [UserLayout, RootLayout] (Inner -> Outer)
            // We wrap: page -> UserLayout(page) -> RootLayout(UserLayout(page))

            string wrappedContent = "page"; // Start with the page variable

            foreach (var layout in collectedLayouts)
            {
                // Each step wraps the previous content
                wrappedContent = GenerateConstruction(layout, wrappedContent);
            }

            sb.AppendLine($"                return {wrappedContent};");
            sb.AppendLine("            });");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("RouteRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private void GenerateParameterInjection(StringBuilder sb, INamedTypeSymbol pageType)
    {
        var props = pageType.GetMembers().OfType<IPropertySymbol>();
        foreach (var prop in props)
        {
            var attr = prop.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "ParameterAttribute");
            if (attr != null)
            {
                string paramName = prop.Name;
                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s)
                {
                    paramName = s;
                }

                // Assume implicit convention is lowercase
                if (attr.ConstructorArguments.Length == 0 || attr.ConstructorArguments[0].Value == null)
                {
                    paramName = paramName.ToLowerInvariant();
                }

                // Generate extraction
                var typeName = prop.Type.ToDisplayString();

                sb.AppendLine($"                if (p.TryGetValue(\"{paramName}\", out var val_{prop.Name}))");
                sb.AppendLine("                {");

                if (typeName == "string")
                {
                    sb.AppendLine($"                    page.{prop.Name} = val_{prop.Name};");
                }
                else if (typeName == "int" || typeName == "System.Int32")
                {
                    sb.AppendLine($"                    if(int.TryParse(val_{prop.Name}, out var parsed_{prop.Name})) page.{prop.Name} = parsed_{prop.Name};");
                }
                else if (typeName == "long" || typeName == "System.Int64")
                {
                    sb.AppendLine($"                    if(long.TryParse(val_{prop.Name}, out var parsed_{prop.Name})) page.{prop.Name} = parsed_{prop.Name};");
                }
                else if (typeName == "System.Guid")
                {
                    sb.AppendLine($"                    if(System.Guid.TryParse(val_{prop.Name}, out var parsed_{prop.Name})) page.{prop.Name} = parsed_{prop.Name};");
                }
                else if (typeName == "bool" || typeName == "System.Boolean")
                {
                    sb.AppendLine($"                    if(bool.TryParse(val_{prop.Name}, out var parsed_{prop.Name})) page.{prop.Name} = parsed_{prop.Name};");
                }
                else if (typeName == "double" || typeName == "System.Double")
                {
                    sb.AppendLine($"                    if(double.TryParse(val_{prop.Name}, out var parsed_{prop.Name})) page.{prop.Name} = parsed_{prop.Name};");
                }

                sb.AppendLine("                }");
            }
        }
    }

    private bool InheritsFrom(INamedTypeSymbol symbol, string baseName)
    {
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == baseName) return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private string GenerateConstruction(INamedTypeSymbol symbol, string? injectedContent)
    {
        var ctor = symbol.Constructors
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (ctor == null || ctor.Parameters.Length == 0)
        {
            return $"new {symbol.ToDisplayString()}()";
        }

        var args = new List<string>();
        foreach (var param in ctor.Parameters)
        {
            if (injectedContent != null && IsControl(param.Type))
            {
                args.Add(injectedContent);
                injectedContent = null;
            }
            else
            {
                var typeName = param.Type.ToDisplayString();
                args.Add($"services.GetRequiredService<{typeName}>()");
            }
        }

        return $"new {symbol.ToDisplayString()}({string.Join(", ", args)})";
    }

    private bool IsControl(ITypeSymbol type)
    {
        if (type.Name == "Control") return true;
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "Control") return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private string GenerateRoute(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace.ToString();
        var parts = ns.Split('.').ToList();

        var pagesIndex = parts.IndexOf("Pages");
        var pathParts = new List<string>();

        if (pagesIndex != -1 && pagesIndex < parts.Count - 1)
        {
            for (int i = pagesIndex + 1; i < parts.Count; i++)
            {
                pathParts.Add(parts[i].ToLowerInvariant());
            }
        }

        var name = symbol.Name;
        if (name.EndsWith("Page")) name = name.Substring(0, name.Length - 4);

        if (!string.IsNullOrEmpty(name) && name != "Index")
        {
            if (name.StartsWith("Param_"))
            {
                var paramName = name.Substring(6);
                if (paramName.EndsWith("_")) paramName = paramName.Substring(0, paramName.Length - 1);

                pathParts.Add($"{{{paramName.ToLowerInvariant()}}}");
            }
            else
            {
                pathParts.Add(name.ToLowerInvariant());
            }
        }

        var route = "/" + string.Join("/", pathParts);
        return route;
    }
}