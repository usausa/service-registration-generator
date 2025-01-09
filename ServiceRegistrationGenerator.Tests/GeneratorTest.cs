namespace ServiceRegistrationGenerator.Tests;

using Microsoft.Extensions.DependencyInjection;

using ServiceRegistrationGenerator.Attributes;

public class GeneratorTest
{
    [Fact]
    public void TestService()
    {
        using var provider = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();

        // TODO
        Assert.NotNull(provider.GetService<TestService>());
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
