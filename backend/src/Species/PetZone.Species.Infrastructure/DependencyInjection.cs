using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetZone.Core;
using PetZone.Species.Infrastructure.BackgroundServices;
using PetZone.Species.Infrastructure.Options;
using PetZone.Species.Infrastructure.Queries;
using PetZone.Species.Infrastructure.Repositories;

namespace PetZone.Species.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSpeciesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<SpeciesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        // UnitOfWork
        services.AddKeyedScoped<IUnitOfWork, SpeciesUnitOfWork>(UnitOfWorkKey.Species);

        // Repositories
        services.AddScoped<SpeciesRepository>();

        // Query Handlers
        services.AddScoped<GetAllSpeciesHandler>();
        services.AddScoped<GetBreedsBySpeciesIdHandler>();
        services.AddScoped<DeleteSpeciesService>();
        services.AddScoped<DeleteBreedService>();
        services.AddScoped<CreateSpeciesService>();
        services.AddScoped<CreateBreedService>();

        // Options
        var softDeleteOptions = new SoftDeleteOptions();
        configuration.GetSection(SoftDeleteOptions.SectionName).Bind(softDeleteOptions);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(softDeleteOptions));

        // Background Services
        services.AddHostedService<SoftDeleteCleanupService>();

        return services;
    }
}