namespace PetZone.VolunteerRequests.Domain;

public record VolunteerInfo(
    int Experience,
    string Motivation,
    List<string> Certificates,
    List<string> Requisites
);