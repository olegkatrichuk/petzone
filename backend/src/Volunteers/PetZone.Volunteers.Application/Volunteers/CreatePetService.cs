using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Events;

namespace PetZone.Volunteers.Application.Volunteers;

public class CreatePetService(
    IVolunteerRepository volunteerRepository,
    ISpeciesRepository speciesRepository,
    IPublisher publisher,
    ILogger<CreatePetService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreatePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating pet for volunteer {VolunteerId}", command.VolunteerId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var req = command.Request;

        var breedExists = await speciesRepository.BreedExistsAsync(req.SpeciesId, req.BreedId, cancellationToken);
        if (!breedExists)
            return (ErrorList)Error.Validation("pet.breed_not_found", "Указанная порода или вид не существуют.");

        var healthResult = HealthInfo.Create(req.HealthDescription, req.DietOrAllergies ?? "");
        if (healthResult.IsFailure) return (ErrorList)healthResult.Error;

        var addressResult = Address.Create(req.City, req.Street);
        if (addressResult.IsFailure) return (ErrorList)addressResult.Error;

        var weightResult = Weight.Create(req.Weight);
        if (weightResult.IsFailure) return (ErrorList)weightResult.Error;

        var heightResult = Height.Create(req.Height);
        if (heightResult.IsFailure) return (ErrorList)heightResult.Error;

        var phoneResult = PhoneNumber.Create(req.OwnerPhone);
        if (phoneResult.IsFailure) return (ErrorList)phoneResult.Error;

        var speciesBreedResult = SpeciesBreed.Create(req.SpeciesId, req.BreedId);
        if (speciesBreedResult.IsFailure) return (ErrorList)speciesBreedResult.Error;

        var petResult = Pet.Create(
            Guid.NewGuid(),
            req.Nickname,
            req.GeneralDescription,
            req.Color,
            healthResult.Value,
            addressResult.Value,
            weightResult.Value,
            heightResult.Value,
            phoneResult.Value,
            req.IsCastrated,
            DateTime.SpecifyKind(req.DateOfBirth, DateTimeKind.Utc),
            req.IsVaccinated,
            (HelpStatus)req.Status,
            req.MicrochipNumber,
            command.VolunteerId,
            req.AdoptionConditions,
            speciesBreedResult.Value);

        if (petResult.IsFailure)
            return (ErrorList)petResult.Error;

        var addResult = volunteer.AddPet(petResult.Value);
        if (addResult.IsFailure)
            return (ErrorList)addResult.Error;

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        await publisher.Publish(
            new PetCreatedEvent(petResult.Value.Id, command.VolunteerId),
            cancellationToken);

        logger.LogInformation("Pet created successfully. Id: {PetId}", petResult.Value.Id);

        return petResult.Value.Id;
    }
}