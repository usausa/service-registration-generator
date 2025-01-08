namespace ServiceRegistrationGenerator.Attributes;

using System;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ServiceRegistrationAttribute : Attribute
{
    public Lifetime Lifetime { get; }

    public string Pattern { get; }

    public string Assembly { get; set; } = default!;

    public string Namespace { get; set; } = default!;

#if NET8_0_OR_GREATER
    public ServiceRegistrationAttribute(Lifetime lifetime, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
#else
    public ServiceRegistrationAttribute(Lifetime lifetime, string pattern)
#endif
    {
        Lifetime = lifetime;
        Pattern = pattern;
    }
}
