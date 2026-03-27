using MediatR;
using PetZone.Core;
using PetZone.Volunteers.Application.Events;

namespace PetZone.Volunteers.Infrastructure.Cache;

public class VolunteerCacheInvalidationHandler(ICacheService cacheService) :
    INotificationHandler<VolunteerUpdatedEvent>,
    INotificationHandler<VolunteerDeletedEvent>
{
    public Task Handle(VolunteerUpdatedEvent notification, CancellationToken cancellationToken)
        => cacheService.RemoveByPrefixAsync($"volunteer:{notification.VolunteerId}", cancellationToken);

    public Task Handle(VolunteerDeletedEvent notification, CancellationToken cancellationToken)
        => cacheService.RemoveByPrefixAsync($"volunteer:{notification.VolunteerId}", cancellationToken);
}