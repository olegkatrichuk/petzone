using PetZone.Application.Repositories;
using PetZone.Application.Volunteers.Commands;
using PetZone.Domain.Models;

namespace PetZone.Application.Volunteers;

public class CreateVolunteerService
{
    private readonly IVolunteerRepository _repository;

    public CreateVolunteerService(IVolunteerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateVolunteerCommand command, CancellationToken cancellationToken = default)
    {
        var req = command.Request;

        // 1. Создаем Value Objects
        var fullName = new FullName(req.FirstName, req.LastName, req.Patronymic);
        var email = new Email(req.Email);
        var experience = new Experience(req.ExperienceYears);
        var phone = new PhoneNumber(req.Phone);

        // 2. Создаем Агрегат
        var volunteer = new Volunteer(
            Guid.NewGuid(),
            fullName,
            email,
            req.GeneralDescription,
            experience,
            phone
        );

        // 3. Добавляем коллекции
        foreach (var sn in req.SocialNetworks)
        {
            volunteer.AddSocialNetwork(new SocialNetwork(sn.Name, sn.Link));
        }

        foreach (var r in req.Requisites)
        {
            volunteer.AddRequisite(new Requisite(r.Name, r.Description));
        }

        // 4. Сохраняем в базу
        return await _repository.AddAsync(volunteer, cancellationToken);
    }
}