using MediatR;

namespace PetZone.Volunteers.Application.Events;

public record PetCreatedEvent(Guid PetId, Guid VolunteerId) : INotification;