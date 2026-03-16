using Microsoft.AspNetCore.Mvc;
using PetZone.Application.Volunteers;
using PetZone.Application.Volunteers.Commands;
using PetZone.Contracts.Volunteers;

namespace PetZone.API.Controllers;

[ApiController]
[Route("[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly CreateVolunteerService _createVolunteerService;

    // Внедряем наш сервис из слоя Application
    public VolunteersController(CreateVolunteerService createVolunteerService)
    {
        _createVolunteerService = createVolunteerService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Оборачиваем пришедший DTO-запрос в Команду (строго по заданию!)
        var command = new CreateVolunteerCommand(request);

        // 2. Передаем команду в бизнес-логику и ждем ID созданного волонтера
        var volunteerId = await _createVolunteerService.Handle(command, cancellationToken);

        // 3. Возвращаем статус 200 OK и сгенерированный базой ID
        return Ok(volunteerId);
    }
}