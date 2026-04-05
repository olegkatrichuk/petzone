using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Infrastructure.Queries;

namespace PetZone.Volunteers.Presentation;

[ApiController]
[Route("news/system")]
public class SystemNewsController(GetSystemNewsHandler handler) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<SystemNewsPostDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var items = await handler.Handle(page, pageSize, cancellationToken);
        var total = await handler.CountAsync(cancellationToken);
        return Ok(new { items, totalCount = total });
    }
}