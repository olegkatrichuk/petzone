using Microsoft.Extensions.DependencyInjection;

namespace PetZone.Species.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSpeciesApplication(
        this IServiceCollection services)
    {
        return services;
    }
}