namespace BunnyTail.ServiceRegistration.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidMethodStyle => new(
        id: "RFSR0001",
        title: "Invalid method parameter",
        messageFormat: "Method must be partial extension method.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidMethodParameter => new(
        id: "RFSR0002",
        title: "Invalid method parameter",
        messageFormat: "Parameter type must be IServiceCollection for registration method {0}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidMethodReturnType => new(
        id: "RFSR0003",
        title: "Invalid method return type",
        messageFormat: "Return type must be IServiceCollection for registration method {0}.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
