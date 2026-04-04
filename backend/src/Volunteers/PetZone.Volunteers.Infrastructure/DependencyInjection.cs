using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using PetZone.Core;
using PetZone.Species.Application;
using PetZone.Framework.Files;
using PetZone.Volunteers.Infrastructure.BackgroundServices;
using PetZone.Volunteers.Infrastructure.Cache;
using PetZone.Volunteers.Infrastructure.Options;
using PetZone.Volunteers.Infrastructure.Queries;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Infrastructure.Repositories;

namespace PetZone.Volunteers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVolunteersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<VolunteersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        // UnitOfWork
        services.AddKeyedScoped<IUnitOfWork, VolunteersUnitOfWork>(UnitOfWorkKey.Volunteers);

        // Repositories
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();
        services.AddScoped<INewsPostRepository, NewsPostRepository>();

        // Cross-domain checker
        services.AddScoped<IPetSpeciesChecker, PetSpeciesChecker>();

        // Minio
        var minioOptions = configuration.GetSection(MinioOptions.SectionName).Get<MinioOptions>()!;
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(minioOptions.Endpoint)
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .WithSSL(minioOptions.WithSSL)
            .Build());

        services.AddScoped<IFilesProvider, MinioProvider>();

        // Query Handlers
        services.AddScoped<GetVolunteersHandler>();
        services.AddScoped<GetVolunteerByIdHandler>();
        services.AddScoped<GetPetsHandler>();
        services.AddScoped<GetPetByIdHandler>();
        services.AddScoped<GetNewsByVolunteerHandler>();

        // Options
        services.Configure<SoftDeleteOptions>(opts =>
            configuration.GetSection(SoftDeleteOptions.SectionName).Bind(opts));

        // Cache invalidation handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(PetCacheInvalidationHandler).Assembly));

        // Background Services
        services.AddHostedService<MinioCleanupService>();
        services.AddHostedService<SoftDeleteCleanupService>();

        return services;
    }
}