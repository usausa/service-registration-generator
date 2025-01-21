namespace BunnyTail.ServiceRegistration.Generator.Models;

internal sealed record AttributeModel(
    int Lifetime,
    string Pattern,
    string Assembly,
    string Namespace);
