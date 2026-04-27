using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PetZone.Core;
using PetZone.SharedKernel;
using PetZone.Species.Infrastructure;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetPetByIdHandler(
    VolunteersDbContext dbContext,
    SpeciesDbContext speciesDbContext,
    ICacheService cacheService,
    ILogger<GetPetByIdHandler> logger)
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public async Task<Result<PetDto, ErrorList>> Handle(
        GetPetByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting pet by id {PetId}", query.PetId);

        var cached = await cacheService.GetAsync<PetDto>($"pet:{query.PetId}", cancellationToken);
        if (cached is not null)
            return cached;

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
                x.Pet.Photos,
                OwnerPhone = x.Pet.OwnerPhone.Value,
                x.Pet.ExternalUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rawPet is null)
            return (ErrorList)Error.NotFound("pet.not_found", "Питомец не найден.");

        var species = await speciesDbContext.Species
            .Where(s => s.Id == rawPet.SpeciesId)
            .Include(s => s.Breeds)
            .FirstOrDefaultAsync(cancellationToken);

        var speciesName = species?.GetName("uk");
        var breedName = species?.Breeds.FirstOrDefault(b => b.Id == rawPet.BreedId)?.GetName("uk");

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
                .ToList(),
            rawPet.OwnerPhone,
            rawPet.ExternalUrl,
            speciesName,
            breedName);

        await cacheService.SetAsync($"pet:{query.PetId}", pet, CacheOptions, cancellationToken);

        return pet;
    }
}