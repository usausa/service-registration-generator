namespace ServiceRegistrationGenerator.Tests;

using Microsoft.Extensions.DependencyInjection;

using ServiceRegistrationGenerator;
using ServiceRegistrationGenerator.ExampleLibrary;

public class GeneratorTest
{
    [Fact]
    public void TestService()
    {
        using var provider = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();

        Assert.NotNull(provider.GetService<TestService>());
        Assert.NotNull(provider.GetService<FooService>());
        Assert.NotNull(provider.GetService<IBarService>());
        Assert.NotNull(provider.GetService<IMixedService1>());
        Assert.NotNull(provider.GetService<IMixedService2>());
        Assert.NotNull(provider.GetService<DisposalService>());
        Assert.NotNull(provider.GetService<IBazService>());

        Assert.Equal(provider.GetService<TestService>(), provider.GetService<TestService>());
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
