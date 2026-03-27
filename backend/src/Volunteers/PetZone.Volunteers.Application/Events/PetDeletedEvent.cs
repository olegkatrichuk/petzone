using MediatR;

namespace PetZone.Volunteers.Application.Events;

public record PetDeletedEvent(Guid PetId, Guid VolunteerId) : INotification;