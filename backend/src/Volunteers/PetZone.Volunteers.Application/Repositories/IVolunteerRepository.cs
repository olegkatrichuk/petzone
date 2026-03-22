namespace PetZone.Volunteers.Application.Repositories;

public interface IVolunteerRepository
{
    Task<Guid> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
    
    Task<Volunteer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Guid> SaveAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
    Task<Guid> SoftDeleteAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
    
    // Hard delete — физически удаляет из БД
    Task<Guid> HardDeleteAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
}