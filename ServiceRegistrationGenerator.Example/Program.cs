namespace ServiceRegistrationGenerator.Example;

using ServiceRegistrationGenerator;

using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    public static void Main()
    {
        using var provider = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();
    }
}

internal static partial class ServiceCollectionExtensions
{
    [ServiceRegistration(Lifetime.Singleton, "Service$")]
    [ServiceRegistration(Lifetime.Singleton, "Service$", Assembly = "ServiceRegistrationGenerator.ExampleLibrary")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);
}

#pragma warning disable CA1812
internal sealed class TestService
{
}
