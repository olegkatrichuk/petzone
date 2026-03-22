using Microsoft.Extensions.DependencyInjection;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Infrastructure;

namespace PetZone.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly VolunteersDbContext DbContext;
    protected readonly SpeciesDbContext SpeciesContext;

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<VolunteersDbContext>();
        SpeciesContext = scope.ServiceProvider.GetRequiredService<SpeciesDbContext>();
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        DbContext.Volunteers.RemoveRange(DbContext.Volunteers);
        await DbContext.SaveChangesAsync();

        SpeciesContext.Species.RemoveRange(SpeciesContext.Species);
        await SpeciesContext.SaveChangesAsync();
    }
}