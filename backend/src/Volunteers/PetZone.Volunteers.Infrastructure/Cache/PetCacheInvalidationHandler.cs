using MediatR;
using PetZone.Core;
using PetZone.Volunteers.Application.Events;

namespace PetZone.Volunteers.Infrastructure.Cache;

public class PetCacheInvalidationHandler(ICacheService cacheService) :
    INotificationHandler<PetCreatedEvent>,
    INotificationHandler<PetUpdatedEvent>,
    INotificationHandler<PetDeletedEvent>
{
    // При создании питомца меняется счётчик у волонтёра — инвалидируем волонтёра
    public Task Handle(PetCreatedEvent notification, CancellationToken cancellationToken)
        => cacheService.RemoveByPrefixAsync($"volunteer:{notification.VolunteerId}", cancellationToken);

    public Task Handle(PetUpdatedEvent notification, CancellationToken cancellationToken)
        => cacheService.RemoveByPrefixAsync($"pet:{notification.PetId}", cancellationToken);

    public Task Handle(PetDeletedEvent notification, CancellationToken cancellationToken)
        => cacheService.RemoveByPrefixAsync($"pet:{notification.PetId}", cancellationToken);
}