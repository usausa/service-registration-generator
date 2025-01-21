namespace BunnyTail.ServiceRegistration.Generator.Models;

using BunnyTail.ServiceRegistration.Generator.Helpers;

using Microsoft.CodeAnalysis;

internal sealed record MethodModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    Accessibility MethodAccessibility,
    string MethodName,
    string ParameterName,
    EquatableArray<AttributeModel> Attributes);
