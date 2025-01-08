namespace ServiceRegistrationGenerator.Example;

using ServiceRegistrationGenerator.Attributes;

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
    [ServiceRegistration(Lifetime.Singleton, ".*Service.*")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);
}
