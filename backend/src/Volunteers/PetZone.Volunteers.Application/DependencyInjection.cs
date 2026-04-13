using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PetZone.Volunteers.Application.News;
using PetZone.Volunteers.Application.Volunteers;

namespace PetZone.Volunteers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVolunteersApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateVolunteerService>();
        services.AddScoped<UpdateVolunteerMainInfoService>();
        services.AddScoped<UpdateVolunteerSocialNetworksService>();
        services.AddScoped<UpdateVolunteerRequisitesService>();
        services.AddScoped<DeleteVolunteerService>();
        services.AddScoped<HardDeleteVolunteerService>();
        services.AddScoped<CreatePetService>();
        services.AddScoped<UpdatePetService>();
        services.AddScoped<UpdatePetStatusService>();
        services.AddScoped<DeletePetService>();
        services.AddScoped<HardDeletePetService>();
        services.AddScoped<SetMainPhotoService>();
        services.AddScoped<UploadPetPhotosService>();
        services.AddScoped<DeletePetPhotosService>();
        services.AddScoped<MovePetService>();
        services.AddScoped<UploadVolunteerPhotoService>();

        services.AddScoped<CreateAdoptionApplicationHandler>();
        services.AddScoped<UpdateApplicationStatusHandler>();

        services.AddScoped<CreateNewsPostService>();
        services.AddScoped<UpdateNewsPostService>();
        services.AddScoped<DeleteNewsPostService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}