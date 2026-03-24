namespace PetZone.Accounts.Contracts.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    ParticipantAccountDto? ParticipantAccount,
    VolunteerAccountDto? VolunteerAccount,
    AdminAccountDto? AdminAccount
);

public record ParticipantAccountDto(
    Guid Id,
    List<Guid> FavoritePets
);

public record VolunteerAccountDto(
    Guid Id,
    int Experience,
    List<string> Certificates,
    List<string> Requisites
);

public record AdminAccountDto(
    Guid Id
);