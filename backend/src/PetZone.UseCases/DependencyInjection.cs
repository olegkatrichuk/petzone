using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PetZone.UseCases.Volunteers;

namespace PetZone.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateVolunteerService>();
        services.AddScoped<UpdateVolunteerMainInfoService>();
        services.AddScoped<UpdateVolunteerSocialNetworksService>();
        services.AddScoped<UpdateVolunteerRequisitesService>();
        services.AddScoped<DeleteVolunteerService>();
        services.AddScoped<HardDeleteVolunteerService>();
        services.AddScoped<CreatePetService>();
        services.AddScoped<UploadPetPhotosService>();
        services.AddScoped<DeletePetPhotosService>();
        services.AddScoped<MovePetService>();
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}