using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PetZone.Volunteers.Contracts;
using SpeciesEntity = PetZone.Species.Domain.Species;
using PetZone.Species.Domain;

namespace PetZone.IntegrationTests.Volunteers;

public class PetsControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory)
{
    private readonly Guid _speciesId = Guid.NewGuid();
    private readonly Guid _breedId = Guid.NewGuid();

    // --- CREATE PET ---

    [Fact]
    public async Task CreatePet_ValidRequest_ShouldReturn200()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();

        var response = await Client.PostAsJsonAsync(
            $"/volunteers/{volunteerId}/pets", CreatePetRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePet_InvalidBreed_ShouldReturn400()
    {
        var volunteerId = await CreateVolunteerAsync();

        var request = CreatePetRequest() with
        {
            SpeciesId = Guid.NewGuid(),
            BreedId = Guid.NewGuid()
        };

        var response = await Client.PostAsJsonAsync(
            $"/volunteers/{volunteerId}/pets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePet_VolunteerNotFound_ShouldReturn404()
    {
        await SeedSpeciesAsync();

        var response = await Client.PostAsJsonAsync(
            $"/volunteers/{Guid.NewGuid()}/pets", CreatePetRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- UPDATE PET ---

    [Fact]
    public async Task UpdatePet_ValidRequest_ShouldReturn200()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();
        var petId = await CreatePetAsync(volunteerId);

        var updateRequest = new UpdatePetRequest(
            "UpdatedBuddy", "Updated description", "Black",
            "Healthy", null, "Lviv", "New Street",
            30.0, 60.0, "+380991234567",
            false, new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            true, 1, null, null, _speciesId, _breedId);

        var response = await Client.PutAsJsonAsync(
            $"/volunteers/{volunteerId}/pets/{petId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- UPDATE STATUS ---

    [Fact]
    public async Task UpdatePetStatus_ValidStatus_ShouldReturn200()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();
        var petId = await CreatePetAsync(volunteerId);

        var response = await Client.PutAsJsonAsync(
            $"/volunteers/{volunteerId}/pets/{petId}/status",
            new UpdatePetStatusRequest(1));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePetStatus_FoundHome_ShouldReturn400()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();
        var petId = await CreatePetAsync(volunteerId);

        var response = await Client.PutAsJsonAsync(
            $"/volunteers/{volunteerId}/pets/{petId}/status",
            new UpdatePetStatusRequest(2));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- SOFT DELETE ---

    [Fact]
    public async Task DeletePet_ExistingPet_ShouldReturn200()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();
        var petId = await CreatePetAsync(volunteerId);

        var response = await Client.DeleteAsync(
            $"/volunteers/{volunteerId}/pets/{petId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- HARD DELETE ---

    [Fact]
    public async Task HardDeletePet_ExistingPet_ShouldReturn200()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();
        var petId = await CreatePetAsync(volunteerId);

        var response = await Client.DeleteAsync(
            $"/volunteers/{volunteerId}/pets/{petId}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- MOVE POSITION ---

    [Fact]
    public async Task MovePet_ValidPosition_ShouldReturn200()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();

        var pet1Id = await CreatePetAsync(volunteerId, "Pet1");
        var pet2Id = await CreatePetAsync(volunteerId, "Pet2");

        var response = await Client.PutAsJsonAsync(
            $"/volunteers/{volunteerId}/pets/{pet1Id}/position",
            new MovePetRequest(2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MovePet_InvalidPosition_ShouldReturn400()
    {
        var volunteerId = await CreateVolunteerAsync();
        await SeedSpeciesAsync();
        var petId = await CreatePetAsync(volunteerId);

        var response = await Client.PutAsJsonAsync(
            $"/volunteers/{volunteerId}/pets/{petId}/position",
            new MovePetRequest(99));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- HELPERS ---

    private async Task<Guid> CreateVolunteerAsync(string email = "john@test.com")
    {
        var request = new CreateVolunteerRequest(
            "John", "Doe", "", email,
            "Test volunteer", 3, "+380991234567", [], []);

        var response = await Client.PostAsJsonAsync("/Volunteers", request);
        var content = await response.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();
        return content!.Result;
    }

    private async Task<Guid> CreatePetAsync(Guid volunteerId, string nickname = "Buddy")
    {
        var request = CreatePetRequest() with { Nickname = nickname };
        var response = await Client.PostAsJsonAsync(
            $"/volunteers/{volunteerId}/pets", request);
        var content = await response.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();
        return content!.Result;
    }

    private async Task SeedSpeciesAsync()
    {
        var speciesResult = SpeciesEntity.Create(_speciesId, "Собака");
        var breedResult = Breed.Create(_breedId, "Лабрадор");
        speciesResult.Value.AddBreed(breedResult.Value);
        SpeciesContext.Species.Add(speciesResult.Value);
        await SpeciesContext.SaveChangesAsync();
    }

    private CreatePetRequest CreatePetRequest() =>
        new(
            "Buddy", "Test description", "Golden",
            "Healthy", null, "Kyiv", "Main Street",
            25.5, 55.0, "+380991234567",
            false, new DateTime(2022, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            true, 1, null, null, _speciesId, _breedId);
}