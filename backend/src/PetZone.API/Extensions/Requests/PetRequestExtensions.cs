using PetZone.Contracts.Volunteers;
using PetZone.UseCases.Commands;

namespace PetZone.API.Extensions.Requests;

public static class PetRequestExtensions
{
    public static CreatePetCommand ToCommand(
        this CreatePetRequest request, Guid volunteerId)
        => new(volunteerId, request);

    public static MovePetCommand ToCommand(
        this MovePetRequest request, Guid volunteerId, Guid petId)
        => new(volunteerId, petId, request);
}