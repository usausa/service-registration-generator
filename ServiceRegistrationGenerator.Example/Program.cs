namespace ServiceRegistrationGenerator.Example;

using Microsoft.Extensions.DependencyInjection;

using ServiceRegistrationGenerator.Attributes;

internal static class Program
{
    public static void Main()
    {
        using var provider = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();
    }
}

internal static class ServiceCollectionExtensions
{
    [ServiceRegistration(".*Service.*")]
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // TODO
        return services;
    }
}
