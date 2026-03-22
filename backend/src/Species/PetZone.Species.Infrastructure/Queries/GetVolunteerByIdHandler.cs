using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace PetZone.Species.Infrastructure.Queries;

public class GetVolunteerByIdHandler(
    ReadDbContext dbContext,
    ILogger<GetVolunteerByIdHandler> logger)
{
    public async Task<Result<VolunteerDto, ErrorList>> Handle(
        GetVolunteerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting volunteer by id {VolunteerId}", query.VolunteerId);

        var volunteer = await dbContext.Volunteers
            .Where(v => !v.IsDeleted && v.Id == query.VolunteerId)
            .Select(v => new VolunteerDto(
                v.Id,
                v.Name.FirstName,
                v.Name.LastName,
                v.Name.Patronymic,
                v.Email.Value,
                v.Phone.Value,
                v.Experience.Years,
                v.GeneralDescription,
                v.Pets.Count(p => !p.IsDeleted),
                v.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);

        if (volunteer is null)
        {
            logger.LogWarning("Volunteer {VolunteerId} not found", query.VolunteerId);
            return (ErrorList)Error.NotFound("volunteer.not_found", "Волонтёр не найден.");
        }

        return volunteer;
    }
}