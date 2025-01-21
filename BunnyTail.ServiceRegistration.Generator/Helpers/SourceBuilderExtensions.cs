namespace BunnyTail.ServiceRegistration.Generator.Helpers;

internal static class SourceBuilderExtensions
{
    public static SourceBuilder BeginScope(this SourceBuilder builder)
    {
        builder.Indent().Append('{').NewLine();
        builder.IndentLevel++;
        return builder;
    }

    public static SourceBuilder EndScope(this SourceBuilder builder)
    {
        builder.IndentLevel--;
        builder.Indent().Append('}').NewLine();
        return builder;
    }

    public static SourceBuilder AutoGenerated(this SourceBuilder builder) =>
        builder.Indent().Append("// <auto-generated />").NewLine();

    public static SourceBuilder EnableNullable(this SourceBuilder builder) =>
        builder.Indent().Append("#nullable enable").NewLine();

    public static SourceBuilder Namespace(this SourceBuilder builder, string ns) =>
        builder.Indent().Append("namespace ").Append(ns).Append(';').NewLine();

    public static SourceBuilder Using(this SourceBuilder builder, string ns) =>
        builder.Indent().Append("using ").Append(ns).Append(';').NewLine();
}
