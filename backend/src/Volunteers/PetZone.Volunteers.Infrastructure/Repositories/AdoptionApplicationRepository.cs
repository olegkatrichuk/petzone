using Microsoft.EntityFrameworkCore;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Repositories;

public class AdoptionApplicationRepository(VolunteersDbContext dbContext) : IAdoptionApplicationRepository
{
    public async Task AddAsync(AdoptionApplication application, CancellationToken cancellationToken = default)
    {
        await dbContext.AdoptionApplications.AddAsync(application, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdoptionApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdoptionApplications
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid petId, Guid applicantUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdoptionApplications
            .AnyAsync(a => a.PetId == petId && a.ApplicantUserId == applicantUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<AdoptionApplication>> GetByVolunteerIdAsync(
        Guid volunteerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdoptionApplications
            .Where(a => a.VolunteerId == volunteerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdoptionApplication>> GetByApplicantIdAsync(
        Guid applicantUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AdoptionApplications
            .Where(a => a.ApplicantUserId == applicantUserId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
