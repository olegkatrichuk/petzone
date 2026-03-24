namespace PetZone.VolunteerRequests.Domain;

public record VolunteerInfo(
    int Experience,
    List<string> Certificates,
    List<string> Requisites
);