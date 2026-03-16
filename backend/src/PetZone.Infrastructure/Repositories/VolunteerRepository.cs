using PetZone.Application.Repositories;
using PetZone.Domain.Models;

namespace PetZone.Infrastructure.Repositories;

public class VolunteerRepository : IVolunteerRepository
{
    private readonly ApplicationDbContext _dbContext;

    public VolunteerRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        // Добавляем сущность в контекст EF Core
        await _dbContext.Volunteers.AddAsync(volunteer, cancellationToken);
        
        // Сохраняем изменения физически в базу данных
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return volunteer.Id;
    }
}