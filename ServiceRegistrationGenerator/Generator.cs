namespace ServiceRegistrationGenerator;

using System.Collections.Immutable;
using System.Text;

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

    // ------------------------------------------------------------
    // Model
    // ------------------------------------------------------------

    internal sealed record AttributeModel(
        string Lifetime,
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
                // TODO validate
                continue;
            }

            if (methodSymbol.ReturnType is not INamedTypeSymbol returnTypeSymbol)
            {
                continue;
            }

            // Validate return type
            if (!IsServiceCollectionName(returnTypeSymbol.ToDisplayString()))
            {
                // TODO validate
                continue;
            }

            // Build source
            var containingType = methodSymbol.ContainingType;
            var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
                ? string.Empty
                : containingType.ContainingNamespace.ToDisplayString();
            BuildSource(
                buffer,
                ns,
                methodSymbol,
                EnumerateAttribute(methodSymbol));

            var source = buffer.ToString();
            var filename = MakeRegistryFilename(buffer, ns, containingType.Name);
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static IEnumerable<AttributeModel> EnumerateAttribute(IMethodSymbol methodSymbol)
    {
        foreach (var attributeData in methodSymbol.GetAttributes().Where(static x => IsServiceRegistrationAttributeName(x.AttributeClass?.Name)))
        {
            var lifetime = attributeData.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
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
        StringBuilder buffer,
        string ns,
        IMethodSymbol methodSymbol,
        IEnumerable<AttributeModel> attributes)
    {
        // TODO ref cache
        // TODO this ref
        // TODO Builder?

        buffer.AppendLine("// <auto-generated />");
        buffer.AppendLine("#nullable enable");

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append("namespace ").Append(ns).AppendLine();
            buffer.AppendLine("{");
        }

        // class
        buffer.Append("    ");
        buffer.Append(methodSymbol.ContainingType.IsStatic ? "static " : " ");
        buffer.Append("partial ");
        buffer.Append(methodSymbol.ContainingType.IsValueType ? "struct " : "class ");
        buffer.Append(methodSymbol.ContainingType.Name);
        buffer.AppendLine();
        buffer.AppendLine("    {");

        // method
        buffer.Append("        ");
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

        buffer.AppendLine("        {");

        // TODO
        foreach (var attribute in attributes)
        {
            buffer.Append("            ");
            buffer.AppendLine($"// {attribute.Pattern}");
        }

        buffer.Append("            return ");
        buffer.Append(methodSymbol.Parameters[0].Name);
        buffer.AppendLine(";");
        buffer.AppendLine("        }");

        buffer.AppendLine("    }");

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.AppendLine("}");
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
