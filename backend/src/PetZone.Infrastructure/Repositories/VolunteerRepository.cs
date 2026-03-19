using Microsoft.EntityFrameworkCore;
using PetZone.Domain.Models;
using PetZone.UseCases.Repositories;

namespace PetZone.Infrastructure.Repositories;

public class VolunteerRepository : IVolunteerRepository
{
    private readonly ApplicationDbContext _dbContext;
    private IVolunteerRepository _volunteerRepositoryImplementation;

    public VolunteerRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Volunteers.AddAsync(volunteer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }

    public async Task<Volunteer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Volunteers
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Guid> SaveAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        // EF Core уже отслеживает сущность — просто сохраняем изменения
        _dbContext.Volunteers.Attach(volunteer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }

    public async Task<Guid> SoftDeleteAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        // Volunteer.Delete() уже вызван в сервисе — просто сохраняем изменения
        _dbContext.Volunteers.Attach(volunteer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }

    public async Task<Guid> HardDeleteAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        // Физически удаляем из БД
        _dbContext.Volunteers.Remove(volunteer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }
}