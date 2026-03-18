// PetZone.UseCases/Volunteers/CreateVolunteerService.cs
using CSharpFunctionalExtensions;
using FluentValidation;
using PetZone.Domain.Models;
using PetZone.Domain.Shared;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Repositories;

namespace PetZone.UseCases.Volunteers;

public class CreateVolunteerService(
    IVolunteerRepository repository,
    IValidator<CreateVolunteerCommand> validator)  // ← инжектируем validator
{
    public async Task<Result<Guid, Error>> Handle(
        CreateVolunteerCommand command, 
        CancellationToken cancellationToken = default)
    {
        // 1. Валидация ПЕРЕД бизнес-логикой
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var first = validationResult.Errors.First();
            return Error.Validation(first.ErrorCode, first.ErrorMessage);
        }

        var req = command.Request;

        // 2. Создаём Value Objects — здесь уже НЕТ проверок IsFailure,
        //    потому что validator уже гарантировал валидность данных
        var email = Email.Create(req.Email).Value;
        var fullName = FullName.Create(req.FirstName, req.LastName, req.Patronymic).Value;
        var experience = Experience.Create(req.ExperienceYears).Value;
        var phone = PhoneNumber.Create(req.Phone).Value;

        // 3. Создаём агрегат
        var volunteerResult = Volunteer.Create(
            Guid.NewGuid(), fullName, email,
            req.GeneralDescription, experience, phone);

        if (volunteerResult.IsFailure)
            return volunteerResult.Error;

        var volunteer = volunteerResult.Value;

        // 4. Добавляем коллекции
        foreach (var sn in req.SocialNetworks)
            volunteer.AddSocialNetwork(SocialNetwork.Create(sn.Name, sn.Link).Value);

        foreach (var r in req.Requisites)
            volunteer.AddRequisite(Requisite.Create(r.Name, r.Description).Value);

        // 5. Сохраняем
        await repository.AddAsync(volunteer, cancellationToken);

        return volunteer.Id;
    }
}