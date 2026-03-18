using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PetZone.UseCases.Volunteers;

namespace PetZone.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateVolunteerService>();
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}