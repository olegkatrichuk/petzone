using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Infrastructure;
using Testcontainers.PostgreSql;

namespace PetZone.IntegrationTests;

public class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("petzone_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();

            // Замінюємо VolunteersDbContext
            services.RemoveAll<VolunteersDbContext>();
            services.AddScoped<VolunteersDbContext>(_ =>
                new VolunteersDbContext(
                    new DbContextOptionsBuilder<VolunteersDbContext>()
                        .UseNpgsql(_dbContainer.GetConnectionString())
                        .Options));

            // Замінюємо SpeciesDbContext
            services.RemoveAll<SpeciesDbContext>();
            services.AddScoped<SpeciesDbContext>(_ =>
                new SpeciesDbContext(
                    new DbContextOptionsBuilder<SpeciesDbContext>()
                        .UseNpgsql(_dbContainer.GetConnectionString())
                        .Options));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();

        // Міграції через VolunteersDbContext
        var volunteersDb = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        await volunteersDb.Database.MigrateAsync();

        // Міграції через SpeciesDbContext
        var speciesDb = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();
        await speciesDb.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}