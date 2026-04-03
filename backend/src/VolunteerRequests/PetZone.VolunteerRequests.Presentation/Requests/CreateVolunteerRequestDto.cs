namespace PetZone.VolunteerRequests.Presentation.Requests;

public record CreateVolunteerRequestDto(
    int Experience,
    string Motivation,
    List<string> Certificates,
    List<string> Requisites
);