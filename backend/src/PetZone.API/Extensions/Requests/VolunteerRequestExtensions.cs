using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Application.Commands;

namespace PetZone.API.Extensions.Requests;

public static class VolunteerRequestExtensions
{
    public static CreateVolunteerCommand ToCommand(this CreateVolunteerRequest request, Guid userId)
        => new(request, userId);

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