using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Application.Repositories;

public interface IAdoptionApplicationRepository
{
    Task AddAsync(AdoptionApplication application, CancellationToken cancellationToken = default);
    Task<AdoptionApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid petId, Guid applicantUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdoptionApplication>> GetByVolunteerIdAsync(Guid volunteerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdoptionApplication>> GetByApplicantIdAsync(Guid applicantUserId, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}