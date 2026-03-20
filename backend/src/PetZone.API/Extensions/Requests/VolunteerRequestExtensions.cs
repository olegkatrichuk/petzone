using PetZone.Contracts.Volunteers;
using PetZone.UseCases.Commands;

namespace PetZone.API.Extensions.Requests;

public static class VolunteerRequestExtensions
{
    public static CreateVolunteerCommand ToCommand(this CreateVolunteerRequest request)
        => new(request);

    public static UpdateVolunteerMainInfoCommand ToCommand(
        this UpdateVolunteerMainInfoRequest request, Guid volunteerId)
        => new(volunteerId, request);

    public static UpdateVolunteerSocialNetworksCommand ToCommand(
        this UpdateVolunteerSocialNetworksRequest request, Guid volunteerId)
        => new(volunteerId, request);

    public static UpdateVolunteerRequisitesCommand ToCommand(
        this UpdateVolunteerRequisitesRequest request, Guid volunteerId)
        => new(volunteerId, request);

    public static DeleteVolunteerCommand ToCommand(this Guid volunteerId)
        => new(volunteerId);

    public static HardDeleteVolunteerCommand ToHardDeleteCommand(this Guid volunteerId)
        => new(volunteerId);
}