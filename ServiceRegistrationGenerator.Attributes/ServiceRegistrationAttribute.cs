namespace ServiceRegistrationGenerator.Attributes;

using System;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ServiceRegistrationAttribute : Attribute
{
    public string Pattern { get; }

    public string Assembly { get; set; } = default!;

    public string Namespace { get; set; } = default!;

#if NET8_0_OR_GREATER
    public ServiceRegistrationAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
#else
    public ServiceRegistrationAttribute(string pattern)
#endif
    {
        Pattern = pattern;
    }
}
