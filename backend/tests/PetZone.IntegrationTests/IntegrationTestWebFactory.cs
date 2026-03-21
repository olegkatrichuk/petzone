using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PetZone.Infrastructure;
using Testcontainers.PostgreSql;

namespace PetZone.IntegrationTests;

public  class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
            // Отключаем background services
            services.RemoveAll<IHostedService>();

            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<ReadDbContext>();

            services.AddScoped<ApplicationDbContext>(_ =>
                new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseNpgsql(_dbContainer.GetConnectionString())
                        .Options));

            services.AddScoped<ReadDbContext>(_ =>
                new ReadDbContext(
                    new DbContextOptionsBuilder<ReadDbContext>()
                        .UseNpgsql(_dbContainer.GetConnectionString())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .Options));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}