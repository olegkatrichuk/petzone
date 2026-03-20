using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class CreatePetService(
    IVolunteerRepository volunteerRepository,
    ISpeciesRepository speciesRepository,
    ILogger<CreatePetService> logger)
{
    public async Task<Result<Guid, Error>> Handle(
        CreatePetCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating pet for volunteer {VolunteerId}", command.VolunteerId);

        // 1. Находим волонтёра
        var volunteer = await volunteerRepository.GetByIdAsync(command.VolunteerId, cancellationToken);
        if (volunteer is null)
            return Error.NotFound("volunteer.not_found", "Волонтёр не найден.");

        var req = command.Request;

        // 2. Проверяем что вид и порода существуют
        var breedExists = await speciesRepository.BreedExistsAsync(req.SpeciesId, req.BreedId, cancellationToken);
        if (!breedExists)
            return Error.Validation("pet.breed_not_found", "Указанная порода или вид не существуют.");

        // 3. Создаём Value Objects
        var healthResult = HealthInfo.Create(req.HealthDescription, req.DietOrAllergies ?? "");
        if (healthResult.IsFailure) return healthResult.Error;

        var addressResult = Address.Create(req.City, req.Street);
        if (addressResult.IsFailure) return addressResult.Error;

        var weightResult = Weight.Create(req.Weight);
        if (weightResult.IsFailure) return weightResult.Error;

        var heightResult = Height.Create(req.Height);
        if (heightResult.IsFailure) return heightResult.Error;

        var phoneResult = PhoneNumber.Create(req.OwnerPhone);
        if (phoneResult.IsFailure) return phoneResult.Error;

        var speciesBreedResult = SpeciesBreed.Create(req.SpeciesId, req.BreedId);
        if (speciesBreedResult.IsFailure) return speciesBreedResult.Error;

        // 4. Создаём питомца
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
            return petResult.Error;

        // 5. Добавляем питомца через волонтёра
        var addResult = volunteer.AddPet(petResult.Value);
        if (addResult.IsFailure)
            return addResult.Error;

        // 6. Сохраняем
        await volunteerRepository.SaveAsync(volunteer, cancellationToken);

        logger.LogInformation("Pet created successfully. Id: {PetId}", petResult.Value.Id);

        return petResult.Value.Id;
    }
}