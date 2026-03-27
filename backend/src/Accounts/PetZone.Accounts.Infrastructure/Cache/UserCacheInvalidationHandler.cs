using MediatR;
using PetZone.Accounts.Application.Events;
using PetZone.Core;

namespace PetZone.Accounts.Infrastructure.Cache;

public class UserCacheInvalidationHandler(ICacheService cacheService) :
    INotificationHandler<UserCacheInvalidationEvent>
{
    public Task Handle(UserCacheInvalidationEvent notification, CancellationToken cancellationToken)
        => cacheService.RemoveByPrefixAsync($"user:{notification.UserId}", cancellationToken);
}