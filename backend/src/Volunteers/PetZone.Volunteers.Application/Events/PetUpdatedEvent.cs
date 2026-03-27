using MediatR;

namespace PetZone.Volunteers.Application.Events;

public record PetUpdatedEvent(Guid PetId, Guid VolunteerId) : INotification;