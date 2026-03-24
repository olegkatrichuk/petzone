namespace PetZone.VolunteerRequests.Presentation.Requests;

public record UpdateVolunteerRequestDto(
    int Experience,
    List<string> Certificates,
    List<string> Requisites
);