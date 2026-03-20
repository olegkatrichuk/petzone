using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using PetZone.Infrastructure.BackgroundServices;
using PetZone.Infrastructure.Options;
using PetZone.Infrastructure.Providers;
using PetZone.Infrastructure.Repositories;
using PetZone.UseCases.Providers;
using PetZone.UseCases.Repositories;

namespace PetZone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ApplicationDbContext>();
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();
        services.AddHostedService<MinioCleanupService>();

        // Soft delete options
        services.AddOptions<SoftDeleteOptions>()
            .BindConfiguration(SoftDeleteOptions.SectionName);

        // Minio options
        services.AddOptions<MinioOptions>()
            .BindConfiguration(MinioOptions.SectionName);
        
        // Minio client
        services.AddMinio(configureClient =>
        {
            var options = services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<MinioOptions>>().Value;

            configureClient
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(false);
        });

        // Files provider
        services.AddScoped<IFilesProvider, MinioProvider>();

        // Background service
        services.AddHostedService<SoftDeleteCleanupService>();

        return services;
    }
}