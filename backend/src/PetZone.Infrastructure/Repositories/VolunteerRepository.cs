using Microsoft.EntityFrameworkCore;
using PetZone.Domain.Models;
using PetZone.UseCases.Repositories;

namespace PetZone.Infrastructure.Repositories;

public class VolunteerRepository(ApplicationDbContext dbContext) : IVolunteerRepository
{
    public async Task<Guid> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        await dbContext.Volunteers.AddAsync(volunteer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }

    public async Task<Volunteer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Volunteers
            .Include(v => v.Pets)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Guid> SaveAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        foreach (var pet in volunteer.Pets)
        {
            if (dbContext.Entry(pet).State == EntityState.Detached)
                dbContext.Entry(pet).State = EntityState.Added;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }

    public async Task<Guid> SoftDeleteAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }

    public async Task<Guid> HardDeleteAsync(Volunteer volunteer, CancellationToken cancellationToken = default)
    {
        dbContext.Volunteers.Remove(volunteer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return volunteer.Id;
    }
}