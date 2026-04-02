using Microsoft.Extensions.DependencyInjection;

namespace PetZone.Listings.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddListingsApplication(
        this IServiceCollection services)
    {
        return services;
    }
}