using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Application.Commands;

namespace PetZone.API.Extensions.Requests;

public static class PetRequestExtensions
{
    public static CreatePetCommand ToCommand(
        this CreatePetRequest request, Guid volunteerId)
        => new(volunteerId, request);

    public static MovePetCommand ToCommand(
        this MovePetRequest request, Guid volunteerId, Guid petId)
        => new(volunteerId, petId, request);
    
    public static UpdatePetCommand ToCommand(
        this UpdatePetRequest request, Guid volunteerId, Guid petId)
        => new(volunteerId, petId, request);

    public static UpdatePetStatusCommand ToCommand(
        this UpdatePetStatusRequest request, Guid volunteerId, Guid petId)
        => new(volunteerId, petId, request);

    public static SetMainPhotoCommand ToCommand(
        this SetMainPhotoRequest request, Guid volunteerId, Guid petId)
        => new(volunteerId, petId, request);
}