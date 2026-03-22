namespace PetZone.Volunteers.Application.Commands;

public record UpdateVolunteerMainInfoCommand(Guid VolunteerId, UpdateVolunteerMainInfoRequest Request);