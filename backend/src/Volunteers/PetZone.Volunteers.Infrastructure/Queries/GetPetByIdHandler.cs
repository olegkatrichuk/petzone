using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetPetByIdHandler(
    VolunteersDbContext dbContext,
    ILogger<GetPetByIdHandler> logger)
{
    public async Task<Result<PetDto, ErrorList>> Handle(
        GetPetByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting pet by id {PetId}", query.PetId);

        var rawPet = await dbContext.Volunteers
            .Where(v => !v.IsDeleted)
            .SelectMany(v => v.Pets
                .Where(p => !p.IsDeleted && p.Id == query.PetId)
                .Select(p => new { Pet = p, VolunteerId = v.Id }))
            .Select(x => new
            {
                x.Pet.Id,
                x.VolunteerId,
                x.Pet.Nickname,
                x.Pet.Color,
                x.Pet.GeneralDescription,
                City = x.Pet.Location.City,
                Street = x.Pet.Location.Street,
                Weight = x.Pet.Weight.Value,
                Height = x.Pet.Height.Value,
                x.Pet.IsCastrated,
                x.Pet.IsVaccinated,
                x.Pet.DateOfBirth,
                x.Pet.Status,
                x.Pet.MicrochipNumber,
                x.Pet.AdoptionConditions,
                SpeciesId = x.Pet.SpeciesBreedInfo.SpeciesId,
                BreedId = x.Pet.SpeciesBreedInfo.BreedId,
                x.Pet.Position,
                x.Pet.IsDeleted,
                x.Pet.Photos
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rawPet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        var pet = new PetDto(
            rawPet.Id,
            rawPet.VolunteerId,
            rawPet.Nickname,
            rawPet.Color,
            rawPet.GeneralDescription,
            rawPet.City,
            rawPet.Street,
            rawPet.Weight,
            rawPet.Height,
            rawPet.IsCastrated,
            rawPet.IsVaccinated,
            rawPet.DateOfBirth,
            (int)rawPet.Status,
            rawPet.MicrochipNumber,
            rawPet.AdoptionConditions,
            rawPet.SpeciesId,
            rawPet.BreedId,
            rawPet.Position,
            rawPet.IsDeleted,
            rawPet.Photos
                .OrderByDescending(p => p.IsMain)
                .Select(p => new PetPhotoDto(p.FilePath, p.IsMain))
                .ToList());

        return pet;
    }
}