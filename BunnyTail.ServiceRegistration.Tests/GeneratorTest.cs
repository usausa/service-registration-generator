namespace BunnyTail.ServiceRegistration;

using Develop.Library;

using Microsoft.Extensions.DependencyInjection;

public class GeneratorTest
{
    [Fact]
    public void TestService()
    {
        using var provider = new ServiceCollection()
            .AddViews()
            .AddServices()
            .BuildServiceProvider();

        Assert.NotNull(provider.GetService<TestService>());
        Assert.NotNull(provider.GetService<FooService>());
        Assert.NotNull(provider.GetService<IBarService>());
        Assert.NotNull(provider.GetService<IMixedService1>());
        Assert.NotNull(provider.GetService<IMixedService2>());
        Assert.NotNull(provider.GetService<DisposalService>());
        Assert.NotNull(provider.GetService<IBazService>());

        Assert.NotNull(provider.GetService<FooViewModel>());
        Assert.NotNull(provider.GetService<BarViewModel>());
        Assert.Empty(provider.GetServices<INavigation>());

        Assert.Equal(provider.GetService<TestService>(), provider.GetService<TestService>());
    }
}

internal static partial class ServiceCollectionExtensions
{
    [ServiceRegistration(Lifetime.Transient, "View$")]
    [ServiceRegistration(Lifetime.Transient, "ViewModel$")]
    public static partial IServiceCollection AddViews(this IServiceCollection services);

    [ServiceRegistration(Lifetime.Singleton, "Service$")]
    [ServiceRegistration(Lifetime.Singleton, "Service$", Assembly = "Develop.Library")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);
}

#pragma warning disable CA1812
internal sealed class TestService
{
}

internal interface INavigation
{
    void OnNavigate();
}

internal sealed class FooViewModel : INavigation
{
    public void OnNavigate()
    {
    }
}

internal sealed class BarViewModel : INavigation
{
    public void OnNavigate()
    {
    }
}
