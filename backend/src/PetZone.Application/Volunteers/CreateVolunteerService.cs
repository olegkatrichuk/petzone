using CSharpFunctionalExtensions;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class CreateVolunteerService(IVolunteerRepository repository)
{
    public async Task<Result<Guid, Error>> Handle(CreateVolunteerCommand command, CancellationToken cancellationToken = default)
    {
        var req = command.Request;
        
        // 1. Пытаемся создать Email через наш новый безопасный метод
        var emailResult = Email.Create(req.Email);
        if (emailResult.IsFailure) return emailResult.Error;
        var email = emailResult.Value;
        
        // Пытаемся создать FullName через наш новый безопасный метод
        var fullNameResult = FullName.Create(req.FirstName, req.LastName, req.Patronymic);
        if (fullNameResult.IsFailure) return fullNameResult.Error;
        var fullName = fullNameResult.Value;
        
        
        // 3. Проверяем Опыт
        var experienceResult = Experience.Create(req.ExperienceYears);
        if (experienceResult.IsFailure) return experienceResult.Error;
        var experience = experienceResult.Value;
        
        
        // Пытаемся создать телефон
        var phoneResult = PhoneNumber.Create(req.Phone);
        if (phoneResult.IsFailure) return phoneResult.Error;
        var phone = phoneResult.Value;
        
        // 2. Создаем Агрегат через нашу новую безопасную фабрику
        var volunteerResult = Volunteer.Create(
            Guid.NewGuid(),
            fullName,
            email,
            req.GeneralDescription,
            experience,
            phone
        );

// Проверяем, не нарушил ли волонтёр бизнес-правила (например, пустое или слишком длинное описание)
        if (volunteerResult.IsFailure)
        {
            return volunteerResult.Error; // Возвращаем ошибку, если что-то пошло не так
        }

// Достаем нашего готового, на 100% валидного волонтёра
        var volunteer = volunteerResult.Value;
        
        // Добавляем социальные сети (с валидацией каждой)
        foreach (var sn in req.SocialNetworks)
        {
            var snResult = SocialNetwork.Create(sn.Name, sn.Link);
        
            // Если хотя бы одна соцсеть невалидна — прерываем создание волонтера
            if (snResult.IsFailure)
            {
                return snResult.Error;
            }

            volunteer.AddSocialNetwork(snResult.Value);
        }

        // Добавляем реквизиты (с валидацией)
        foreach (var r in req.Requisites)
        {
            var requisiteResult = Requisite.Create(r.Name, r.Description);
        
            if (requisiteResult.IsFailure)
            {
                return requisiteResult.Error;
            }

            volunteer.AddRequisite(requisiteResult.Value);
        }

        // 4. Сохраняем в базу
        await repository.AddAsync(volunteer, cancellationToken);

        // ВОЗВРАЩАЕМ УСПЕХ:
        return Result.Success<Guid, Error>(volunteer.Id);
    }
}