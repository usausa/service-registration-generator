namespace ServiceRegistrationGenerator.Attributes;

using System;
using System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ServiceRegistrationAttribute : Attribute
{
    public string Pattern { get; }

    public string Assembly { get; set; } = default!;

    public string Namespace { get; set; } = default!;

    public ServiceRegistrationAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
    {
        Pattern = pattern;
    }
}
