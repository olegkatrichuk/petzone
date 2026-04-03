namespace PetZone.VolunteerRequests.Presentation.Requests;

public record UpdateVolunteerRequestDto(
    int Experience,
    string Motivation,
    List<string> Certificates,
    List<string> Requisites
);