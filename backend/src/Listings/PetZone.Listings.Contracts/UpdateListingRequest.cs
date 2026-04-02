using System.ComponentModel.DataAnnotations;

namespace PetZone.Listings.Contracts;

public record UpdateListingRequest(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(2000)] string Description,
    [Required] Guid SpeciesId,
    Guid? BreedId,
    [Range(0, 1200)] int AgeMonths,
    [Required][MaxLength(100)] string Color,
    [Required][MaxLength(200)] string City,
    bool Vaccinated,
    bool Castrated,
    [MaxLength(30)] string? Phone
);