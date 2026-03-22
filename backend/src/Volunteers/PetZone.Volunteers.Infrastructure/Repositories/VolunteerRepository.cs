using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Repositories;

public class VolunteerRepository(VolunteersDbContext dbContext) : IVolunteerRepository
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
            var entry = dbContext.Entry(pet);

            if (entry.State == EntityState.Detached)
                entry.State = EntityState.Added;
            else if (entry.State == EntityState.Modified)
            {
                var exists = await dbContext.Set<Pet>()
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == pet.Id, cancellationToken);

                if (!exists)
                    entry.State = EntityState.Added;
            }
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