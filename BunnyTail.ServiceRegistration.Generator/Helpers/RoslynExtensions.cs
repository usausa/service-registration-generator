namespace BunnyTail.ServiceRegistration.Generator.Helpers;

using Microsoft.CodeAnalysis;

internal static class RoslynExtensions
{
    public static string ToText(this Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        Accessibility.Internal => "internal",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => throw new NotSupportedException()
    };

    public static IEnumerable<INamedTypeSymbol> GetTypeMembersRecursive(this INamespaceSymbol namespaceSymbol)
    {
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            yield return typeSymbol;
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var typeSymbol in GetTypeMembersRecursive(nestedNamespace))
            {
                yield return typeSymbol;
            }
        }
    }

    public static IEnumerable<INamedTypeSymbol> GetTypeMembersRecursive(this INamespaceSymbol namespaceSymbol, Func<INamedTypeSymbol, bool> predicate)
    {
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            if (predicate(typeSymbol))
            {
                yield return typeSymbol;
            }
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var typeSymbol in GetTypeMembersRecursive(nestedNamespace, predicate))
            {
                yield return typeSymbol;
            }
        }
    }
}
