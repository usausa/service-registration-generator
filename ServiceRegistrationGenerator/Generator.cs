namespace ServiceRegistrationGenerator;

using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    private const string MethodAttributeShortName = "ServiceRegistration";
    private const string MethodAttributeTypeName = $"{MethodAttributeShortName}Attribute";

    private const string ServiceCollectionFullName = "Microsoft.Extensions.DependencyInjection.IServiceCollection";

    private static DiagnosticDescriptor InvalidMethodParameter => new(
        id: "RFSR0001",
        title: "Invalid method parameter",
        messageFormat: "Parameter type must be IServiceCollection for registration method {0}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static DiagnosticDescriptor InvalidMethodReturnType => new(
        id: "RFSR0002",
        title: "Invalid method return type",
        messageFormat: "Return type must be IServiceCollection for registration method {0}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    // ------------------------------------------------------------
    // Model
    // ------------------------------------------------------------

    internal sealed record AttributeModel(
        int Lifetime,
        string Pattern,
        string Assembly,
        string Namespace);

    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        var sourceProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsMethodTargetSyntax(node),
                static (context, _) => (MethodDeclarationSyntax)context.Node)
            .Where(static x => x is not null)
            .Collect();

        context.RegisterImplementationSourceOutput(
            compilationProvider.Combine(sourceProvider),
            static (context, provider) => Execute(context, provider.Left, provider.Right));
    }

    private static bool IsMethodTargetSyntax(SyntaxNode node) =>
        (node is MethodDeclarationSyntax { AttributeLists.Count: > 0 } methodSyntax) &&
        methodSyntax.Modifiers.Any(SyntaxKind.StaticKeyword) &&
        methodSyntax.AttributeLists.SelectMany(static x => x.Attributes).Any(static x => IsServiceRegistrationAttributeName(x.Name.ToString()));

    // ------------------------------------------------------------
    // Builder
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods)
    {
        var resolveContext = new SymbolResolveContext(compilation);
        var buffer = new StringBuilder(4096);

        foreach (var methodSyntax in methods)
        {
            buffer.Clear();

            var methodSemantic = compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var methodSymbol = methodSemantic.GetDeclaredSymbol(methodSyntax);
            if (methodSymbol is null)
            {
                continue;
            }

            // Validate argument
            if ((methodSymbol.Parameters.Length != 1) ||
                !methodSymbol.IsExtensionMethod ||
                !IsServiceCollectionName(methodSymbol.Parameters[0].Type.ToDisplayString()))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidMethodParameter, methodSyntax.GetLocation(), methodSymbol.Name));
                continue;
            }

            if (methodSymbol.ReturnType is not INamedTypeSymbol returnTypeSymbol)
            {
                continue;
            }

            // Validate return type
            if (!IsServiceCollectionName(returnTypeSymbol.ToDisplayString()))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidMethodReturnType, methodSyntax.GetLocation(), methodSymbol.Name));
                continue;
            }

            // Build source
            var containingType = methodSymbol.ContainingType;
            var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
                ? string.Empty
                : containingType.ContainingNamespace.ToDisplayString();
            BuildSource(resolveContext, buffer, ns, methodSymbol);

            var source = buffer.ToString();
            var filename = MakeRegistryFilename(buffer, ns, containingType.Name);
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static IEnumerable<AttributeModel> EnumerateAttribute(IMethodSymbol methodSymbol)
    {
        foreach (var attributeData in methodSymbol.GetAttributes().Where(static x => IsServiceRegistrationAttributeName(x.AttributeClass?.Name)))
        {
            var lifetime = Convert.ToInt32(attributeData.ConstructorArguments[0].Value, CultureInfo.InvariantCulture);
            var pattern = attributeData.ConstructorArguments[1].Value?.ToString() ?? string.Empty;
            var assembly = string.Empty;
            var ns = string.Empty;

            foreach (var parameter in attributeData.NamedArguments)
            {
                var name = parameter.Key;
                var value = parameter.Value.Value;

                if (String.IsNullOrEmpty(name) || (value is null))
                {
                    continue;
                }

                switch (name)
                {
                    case "Assembly":
                        assembly = value.ToString();
                        break;
                    case "Namespace":
                        ns = value.ToString();
                        break;
                }
            }

            yield return new AttributeModel(lifetime, pattern, assembly, ns);
        }
    }

    // ------------------------------------------------------------
    // Builder
    // ------------------------------------------------------------

    private static void BuildSource(
        SymbolResolveContext context,
        StringBuilder buffer,
        string ns,
        IMethodSymbol methodSymbol)
    {
        buffer.AppendLine("// <auto-generated />");
        buffer.AppendLine("#nullable enable");

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append("namespace ").Append(ns).AppendLine(";");
            buffer.AppendLine();
        }

        // using
        buffer.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        buffer.AppendLine();

        // class
        buffer.Append(methodSymbol.ContainingType.IsStatic ? "static " : " ");
        buffer.Append("partial ");
        buffer.Append(methodSymbol.ContainingType.IsValueType ? "struct " : "class ");
        buffer.Append(methodSymbol.ContainingType.Name);
        buffer.AppendLine();
        buffer.AppendLine("{");

        // method
        buffer.Append("    ");
        buffer.Append(ToAccessibilityText(methodSymbol.DeclaredAccessibility));
        buffer.Append(" static partial ");
        buffer.Append(methodSymbol.ReturnType.ToDisplayString());
        buffer.Append(' ');
        buffer.Append(methodSymbol.Name);
        buffer.Append("(this ");
        buffer.Append(methodSymbol.Parameters[0].Type.ToDisplayString());
        buffer.Append(' ');
        buffer.Append(methodSymbol.Parameters[0].Name);
        buffer.Append(')');
        buffer.AppendLine();

        buffer.AppendLine("    {");

        foreach (var attribute in EnumerateAttribute(methodSymbol))
        {
            var regex = new Regex(attribute.Pattern);

            foreach (var namedTypeSymbol in context.ResolveClasses())
            {
                if ((String.IsNullOrEmpty(attribute.Namespace) ||
                     (attribute.Namespace == namedTypeSymbol.ContainingNamespace.ToDisplayString())) &&
                    regex.IsMatch(namedTypeSymbol.Name))
                {
                    buffer.Append("        ");
                    buffer.Append(methodSymbol.Parameters[0].Name);
                    buffer.Append(".Add");
                    buffer.Append(attribute.Lifetime switch
                    {
                        1 => "Singleton",
                        2 => "Scoped",
                        _ => "Transient"
                    });
                    buffer.Append('<');
                    buffer.Append(namedTypeSymbol.ToDisplayString());
                    buffer.AppendLine(">();");
                }
            }
        }

        buffer.Append("        return ");
        buffer.Append(methodSymbol.Parameters[0].Name);
        buffer.AppendLine(";");
        buffer.AppendLine("    }");

        buffer.AppendLine("}");
    }

    private sealed class SymbolResolveContext
    {
        private readonly Compilation compilation;

        public SymbolResolveContext(Compilation compilation)
        {
            this.compilation = compilation;
        }

        public IEnumerable<INamedTypeSymbol> ResolveClasses() =>
            ResolveClasses(compilation.Assembly.GlobalNamespace);

        private static IEnumerable<INamedTypeSymbol> ResolveClasses(INamespaceSymbol namespaceSymbol)
        {
            foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
            {
                if (typeSymbol.TypeKind == TypeKind.Class)
                {
                    yield return typeSymbol;
                }
            }

            foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (var typeSymbol in ResolveClasses(nestedNamespace))
                {
                    yield return typeSymbol;
                }
            }
        }
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static bool IsServiceRegistrationAttributeName(string? name) =>
        name is MethodAttributeShortName or MethodAttributeTypeName;

    private static bool IsServiceCollectionName(string name) =>
        name == ServiceCollectionFullName;

    private static string ToAccessibilityText(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        Accessibility.Internal => "internal",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => throw new NotSupportedException()
    };

    private static string MakeRegistryFilename(StringBuilder buffer, string ns, string className)
    {
        buffer.Clear();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className);
        buffer.Append(".g.cs");

        return buffer.ToString();
    }
}
