using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Queries;
using PetZone.Volunteers.Contracts;

namespace PetZone.Volunteers.Infrastructure.Queries;

public class GetPetsHandler(
    VolunteersDbContext dbContext,
    ILogger<GetPetsHandler> logger)
{
    public async Task<Result<PagedList<PetDto>, ErrorList>> Handle(
        GetPetsQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting pets with filters");

        var petsQuery = dbContext.Volunteers
            .Where(v => !v.IsDeleted)
            .SelectMany(v => v.Pets
                .Where(p => !p.IsDeleted)
                .Select(p => new { Pet = p, VolunteerId = v.Id }));

        if (query.VolunteerId.HasValue)
            petsQuery = petsQuery.Where(x => x.VolunteerId == query.VolunteerId.Value);

        if (!string.IsNullOrWhiteSpace(query.Nickname))
            petsQuery = petsQuery.Where(x =>
                EF.Functions.ILike(x.Pet.Nickname, $"%{query.Nickname}%"));

        if (!string.IsNullOrWhiteSpace(query.Color))
            petsQuery = petsQuery.Where(x =>
                EF.Functions.ILike(x.Pet.Color, $"%{query.Color}%"));

        if (!string.IsNullOrWhiteSpace(query.City))
            petsQuery = petsQuery.Where(x =>
                EF.Functions.ILike(x.Pet.Location.City, $"%{query.City}%"));

        if (query.SpeciesId.HasValue)
            petsQuery = petsQuery.Where(x =>
                x.Pet.SpeciesBreedInfo.SpeciesId == query.SpeciesId.Value);

        if (query.BreedId.HasValue)
            petsQuery = petsQuery.Where(x =>
                x.Pet.SpeciesBreedInfo.BreedId == query.BreedId.Value);

        if (query.MinAge.HasValue)
            petsQuery = petsQuery.Where(x =>
                x.Pet.DateOfBirth <= DateTime.UtcNow.AddYears(-query.MinAge.Value));

        if (query.MaxAge.HasValue)
            petsQuery = petsQuery.Where(x =>
                x.Pet.DateOfBirth >= DateTime.UtcNow.AddYears(-query.MaxAge.Value));

        if (query.MinWeight.HasValue)
            petsQuery = petsQuery.Where(x => x.Pet.Weight.Value >= query.MinWeight.Value);

        if (query.MaxWeight.HasValue)
            petsQuery = petsQuery.Where(x => x.Pet.Weight.Value <= query.MaxWeight.Value);

        if (query.IsCastrated.HasValue)
            petsQuery = petsQuery.Where(x => x.Pet.IsCastrated == query.IsCastrated.Value);

        if (query.IsVaccinated.HasValue)
            petsQuery = petsQuery.Where(x => x.Pet.IsVaccinated == query.IsVaccinated.Value);

        if (query.Status.HasValue)
            petsQuery = petsQuery.Where(x => (int)x.Pet.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Source))
        {
            petsQuery = query.Source switch
            {
                "ua" or "local" => petsQuery.Where(x => x.Pet.Country == null || x.Pet.Country == "ua"),
                "pl"            => petsQuery.Where(x => x.Pet.Country == "pl"),
                "de"            => petsQuery.Where(x => x.Pet.Country == "de"),
                "fr"            => petsQuery.Where(x => x.Pet.Country == "fr"),
                "imported"      => petsQuery.Where(x => x.Pet.Country != null),
                _               => petsQuery
            };
        }

        petsQuery = query.SortBy?.ToLower() switch
        {
            "nickname" => query.SortDescending
                ? petsQuery.OrderByDescending(x => x.Pet.Nickname)
                : petsQuery.OrderBy(x => x.Pet.Nickname),
            "age" => query.SortDescending
                ? petsQuery.OrderBy(x => x.Pet.DateOfBirth)
                : petsQuery.OrderByDescending(x => x.Pet.DateOfBirth),
            "weight" => query.SortDescending
                ? petsQuery.OrderByDescending(x => x.Pet.Weight.Value)
                : petsQuery.OrderBy(x => x.Pet.Weight.Value),
            "color" => query.SortDescending
                ? petsQuery.OrderByDescending(x => x.Pet.Color)
                : petsQuery.OrderBy(x => x.Pet.Color),
            "city" => query.SortDescending
                ? petsQuery.OrderByDescending(x => x.Pet.Location.City)
                : petsQuery.OrderBy(x => x.Pet.Location.City),
            "status" => query.SortDescending
                ? petsQuery.OrderByDescending(x => x.Pet.Status)
                : petsQuery.OrderBy(x => x.Pet.Status),
            // "id" — sorts by GUID to pseudo-randomly interleave pets from all sources
            "id" => query.SortDescending
                ? petsQuery.OrderByDescending(x => x.Pet.Id)
                : petsQuery.OrderBy(x => x.Pet.Id),
            _ => petsQuery.OrderBy(x => x.Pet.Position)
        };

        var totalCount = await petsQuery.CountAsync(cancellationToken);

        var rawPets = await petsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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
            .ToListAsync(cancellationToken);

        var petDtos = rawPets.Select(x => new PetDto(
            x.Id,
            x.VolunteerId,
            x.Nickname,
            x.Color,
            x.GeneralDescription,
            x.City,
            x.Street,
            x.Weight,
            x.Height,
            x.IsCastrated,
            x.IsVaccinated,
            x.DateOfBirth,
            (int)x.Status,
            x.MicrochipNumber,
            x.AdoptionConditions,
            x.SpeciesId,
            x.BreedId,
            x.Position,
            x.IsDeleted,
            x.Photos
                .OrderByDescending(p => p.IsMain)
                .Select(p => new PetPhotoDto(p.FilePath, p.IsMain))
                .ToList(),
            x.OwnerPhone,
            x.ExternalUrl))
            .ToList();

        return new PagedList<PetDto>(petDtos, totalCount, query.Page, query.PageSize);
    }
}