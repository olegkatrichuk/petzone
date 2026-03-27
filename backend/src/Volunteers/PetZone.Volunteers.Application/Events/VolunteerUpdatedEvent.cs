using MediatR;

namespace PetZone.Volunteers.Application.Events;

public record VolunteerUpdatedEvent(Guid VolunteerId) : INotification;