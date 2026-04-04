using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Application.Repositories;

public interface INewsPostRepository
{
    Task<Guid> AddAsync(NewsPost newsPost, CancellationToken cancellationToken = default);
    Task<NewsPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(NewsPost newsPost, CancellationToken cancellationToken = default);
}
