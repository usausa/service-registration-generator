namespace BunnyTail.ServiceRegistration.Generator.Models;

using Microsoft.CodeAnalysis;

using SourceGenerateHelper;

internal sealed record MethodModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    Accessibility MethodAccessibility,
    string MethodName,
    string ParameterName,
    EquatableArray<AttributeModel> Attributes);
