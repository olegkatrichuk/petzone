namespace PetZone.VolunteerRequests.Presentation.Requests;

public record CreateVolunteerRequestDto(
    int Experience,
    List<string> Certificates,
    List<string> Requisites
);