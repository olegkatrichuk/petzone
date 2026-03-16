using PetZone.Domain.Models;

namespace PetZone.Application.Repositories;

public interface IVolunteerRepository
{
    // Принимает готового волонтёра и возвращает его ID после сохранения в базу
    Task<Guid> AddAsync(Volunteer volunteer, CancellationToken cancellationToken = default);
}