using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PetZone.Contracts.Volunteers;

namespace PetZone.IntegrationTests.Volunteers;

public class VolunteersControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory)
{
    // --- CREATE ---

    [Fact]
    public async Task CreateVolunteer_ValidRequest_ShouldReturn200()
    {
        var request = CreateVolunteerRequest();

        var response = await Client.PostAsJsonAsync("/Volunteers", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateVolunteer_InvalidEmail_ShouldReturn400()
    {
        var request = CreateVolunteerRequest() with { Email = "not-an-email" };

        var response = await Client.PostAsJsonAsync("/Volunteers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateVolunteer_EmptyFirstName_ShouldReturn400()
    {
        var request = CreateVolunteerRequest() with { FirstName = "" };

        var response = await Client.PostAsJsonAsync("/Volunteers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- GET ALL ---

    [Fact]
    public async Task GetVolunteers_ShouldReturnPagedList()
    {
        // Arrange — создаём двух волонтёров
        await Client.PostAsJsonAsync("/Volunteers", CreateVolunteerRequest());
        await Client.PostAsJsonAsync("/Volunteers",
            CreateVolunteerRequest() with { Email = "second@test.com" });

        // Act
        var response = await Client.GetAsync("/Volunteers?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<EnvelopeResponse<PagedListResponse>>();
        content!.Result.Items.Should().HaveCount(2);
        content.Result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetVolunteers_Pagination_ShouldReturnCorrectPage()
    {
        // Создаём 3 волонтёров
        for (int i = 0; i < 3; i++)
            await Client.PostAsJsonAsync("/Volunteers",
                CreateVolunteerRequest() with { Email = $"volunteer{i}@test.com" });

        var response = await Client.GetAsync("/Volunteers?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<EnvelopeResponse<PagedListResponse>>();
        content!.Result.Items.Should().HaveCount(2);
        content.Result.TotalCount.Should().Be(3);
    }

    // --- GET BY ID ---

    [Fact]
    public async Task GetVolunteerById_ExistingId_ShouldReturn200()
    {
        var createResponse = await Client.PostAsJsonAsync("/Volunteers", CreateVolunteerRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();

        var response = await Client.GetAsync($"/Volunteers/{created!.Result}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetVolunteerById_NotExistingId_ShouldReturn404()
    {
        var response = await Client.GetAsync($"/Volunteers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- UPDATE MAIN INFO ---

    [Fact]
    public async Task UpdateMainInfo_ValidRequest_ShouldReturn200()
    {
        var createResponse = await Client.PostAsJsonAsync("/Volunteers", CreateVolunteerRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();

        var updateRequest = new UpdateVolunteerMainInfoRequest(
            "Jane", "Doe", "", "jane@test.com",
            "Updated description", 5, "+380991234568");

        var response = await Client.PutAsJsonAsync(
            $"/Volunteers/{created!.Result}/main-info", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- SOFT DELETE ---

    [Fact]
    public async Task DeleteVolunteer_ExistingId_ShouldReturn200()
    {
        var createResponse = await Client.PostAsJsonAsync("/Volunteers", CreateVolunteerRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();

        var response = await Client.DeleteAsync($"/Volunteers/{created!.Result}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteVolunteer_ShouldNotAppearInList()
    {
        var createResponse = await Client.PostAsJsonAsync("/Volunteers", CreateVolunteerRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();

        await Client.DeleteAsync($"/Volunteers/{created!.Result}");

        var listResponse = await Client.GetAsync("/Volunteers?page=1&pageSize=10");
        var content = await listResponse.Content.ReadFromJsonAsync<EnvelopeResponse<PagedListResponse>>();
        content!.Result.Items.Should().BeEmpty();
    }

    // --- HARD DELETE ---

    [Fact]
    public async Task HardDeleteVolunteer_ExistingId_ShouldReturn200()
    {
        var createResponse = await Client.PostAsJsonAsync("/Volunteers", CreateVolunteerRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<EnvelopeResponse<Guid>>();

        var response = await Client.DeleteAsync($"/Volunteers/{created!.Result}/hard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- HELPERS ---

    private static CreateVolunteerRequest CreateVolunteerRequest() =>
        new(
            "John", "Doe", "",
            "john@test.com",
            "Test volunteer description",
            3, "+380991234567",
            [], []);
}

// Response models для десериализации
public record EnvelopeResponse<T>(T Result, List<ErrorInfoResponse> ErrorInfo);
public record ErrorInfoResponse(string? ErrorCode, string? ErrorMessage, string? InvalidField);
public record PagedListResponse(List<VolunteerDto> Items, int TotalCount, int Page, int PageSize);