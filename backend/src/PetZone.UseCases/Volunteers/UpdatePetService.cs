using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class UpdatePetService(
    IVolunteerRepository volunteerRepository,
    ISpeciesRepository speciesRepository,
    ILogger<UpdatePetService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdatePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating pet {PetId}", command.PetId);

        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var pet = volunteer.Pets.FirstOrDefault(p => p.Id == command.PetId);
        if (pet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        var req = command.Request;

        // Проверяем вид и породу
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

        var updateResult = pet.Update(
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
            req.AdoptionConditions,
            speciesBreedResult.Value);

        if (updateResult.IsFailure)
            return (ErrorList)updateResult.Error;

        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet {PetId} updated successfully", command.PetId);

        return command.PetId;
    }
}