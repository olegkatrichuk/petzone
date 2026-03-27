using MediatR;

namespace PetZone.Volunteers.Application.Events;

public record VolunteerDeletedEvent(Guid VolunteerId) : INotification;