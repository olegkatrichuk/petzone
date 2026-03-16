using System;
using System.Collections.Generic;

namespace PetZone.Contracts.Volunteers;

public record CreateVolunteerRequest(
    string FirstName, 
    string LastName, 
    string Patronymic,
    string Email, 
    string GeneralDescription, 
    int ExperienceYears, 
    string Phone,
    IEnumerable<SocialNetworkDto> SocialNetworks,
    IEnumerable<RequisiteDto> Requisites)
{
    // Гарантируем, что списки никогда не будут null (если клиент их не прислал)
    public IEnumerable<SocialNetworkDto> SocialNetworks { get; init; } = SocialNetworks ?? Array.Empty<SocialNetworkDto>();
    public IEnumerable<RequisiteDto> Requisites { get; init; } = Requisites ?? Array.Empty<RequisiteDto>();
}