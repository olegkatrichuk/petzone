using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Queries.GetStats;
using PetZone.VolunteerRequests.Application.Queries.GetUnreviewedRequests;
using PetZone.VolunteerRequests.Domain;

namespace PetZone.VolunteerRequests.Application.Repositories;

public interface IVolunteerRequestRepository
{
    Task AddAsync(VolunteerRequest request, CancellationToken cancellationToken = default);
    Task<VolunteerRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VolunteerRequest?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<VolunteerRequest>> GetUnreviewedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<VolunteerRequest>> GetByAdminAsync(Guid adminId, VolunteerRequestStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<VolunteerRequest>> GetByUserAsync(Guid userId, VolunteerRequestStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<VolunteerRequestStats> GetStatsAsync(CancellationToken cancellationToken = default);
}