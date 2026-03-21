using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PetZone.Infrastructure;

namespace PetZone.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Очищаем БД после каждого теста
        DbContext.Volunteers.RemoveRange(DbContext.Volunteers);
        DbContext.Species.RemoveRange(DbContext.Species);
        await DbContext.SaveChangesAsync();
    }
}