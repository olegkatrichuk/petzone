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
using Microsoft.Extensions.Options;
using PetZone.Volunteers.Infrastructure.PolandShelters;
using PetZone.Volunteers.Infrastructure.RescueGroups;
using PetZone.Volunteers.Infrastructure.UkrainianShelters;

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
        services.AddScoped<IAdoptionApplicationRepository, AdoptionApplicationRepository>();

        // Cross-domain checker
        services.AddScoped<IPetSpeciesChecker, PetSpeciesChecker>();

        // Cross-module services
        services.AddScoped<IVolunteerAccountService, VolunteerAccountService>();

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
        services.AddScoped<GetSystemNewsHandler>();

        // Options
        services.Configure<SoftDeleteOptions>(opts =>
            configuration.GetSection(SoftDeleteOptions.SectionName).Bind(opts));

        // Cache invalidation handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(PetCacheInvalidationHandler).Assembly));

        // Background Services
        services.AddHostedService<MinioCleanupService>();
        services.AddHostedService<SoftDeleteCleanupService>();
        services.AddHostedService<DailyContentService>();

        // RescueGroups integration
        services.Configure<RescueGroupsOptions>(configuration.GetSection(RescueGroupsOptions.SectionName));
        services.AddHttpClient("RescueGroups", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<RescueGroupsOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", opts.ApiKey);
        });
        services.AddHostedService<RescueGroupsSyncService>();

        // Ukrainian shelter scrapers
        services.AddHttpClient("lkplev", client =>
        {
            client.BaseAddress = new Uri("https://lkplev.com");
            client.DefaultRequestHeaders.Add("User-Agent", "PetZone/1.0");
        });
        services.AddHttpClient("animalsCity", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "PetZone/1.0");
        });
        services.AddHttpClient("olx", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddHostedService<LkplevSyncService>();
        services.AddHostedService<AnimalsCitySyncService>();
        services.AddHostedService<OlxSyncService>();

        // Poland
        services.AddHttpClient("olx-pl", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddHostedService<OlxPlSyncService>();

        return services;
    }
}