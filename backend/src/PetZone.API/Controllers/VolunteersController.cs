using Microsoft.AspNetCore.Mvc;
using PetZone.API.Extensions;
using PetZone.Contracts.Volunteers;
using PetZone.UseCases.Commands;
using PetZone.UseCases.Volunteers;

namespace PetZone.API.Controllers;

[ApiController]
[Route("[controller]")]
public class VolunteersController(CreateVolunteerService createVolunteerService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateVolunteerCommand(request);

        var result = await createVolunteerService.Handle(command, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        
        return Ok(result.Value);
    }
}