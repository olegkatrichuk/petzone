using MediatR;

namespace PetZone.Accounts.Application.Events;

public record UserCacheInvalidationEvent(Guid UserId) : INotification;