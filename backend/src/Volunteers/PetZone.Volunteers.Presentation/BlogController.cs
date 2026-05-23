using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetZone.Volunteers.Application.Blog;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Contracts;
using PetZone.Volunteers.Presentation.Extensions;

namespace PetZone.Volunteers.Presentation;

[ApiController]
[Route("blog")]
public class BlogController(
    BlogPostsService service,
    IBlogPostRepository repository,
    ILogger<BlogController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedBlogPostsDto>> GetList(
        [FromQuery] string? lang,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await repository.GetPagedAsync(lang, page, pageSize, ct);
        var dto = new PagedBlogPostsDto(
            items.Select(p => new BlogPostListItemDto(p.Id, p.Slug, p.Language, p.Title, p.Summary, p.CoverImageUrl, p.CreatedAt)).ToList(),
            total, page, pageSize);
        return Ok(dto);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<BlogPostDto>> GetBySlug(
        [FromRoute] string slug,
        CancellationToken ct = default)
    {
        var post = await repository.GetBySlugAsync(slug, ct);
        if (post is null) return NotFound();
        return Ok(new BlogPostDto(
            post.Id, post.Slug, post.Language, post.Title, post.Summary,
            post.ContentMarkdown, post.CoverImageUrl, post.AuthorUserId,
            post.CreatedAt, post.UpdatedAt));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateBlogPostRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        logger.LogInformation("Admin {UserId} creating blog post {Slug}", userId, request.Slug);
        var result = await service.CreateAsync(
            request.Slug, request.Language, request.Title, request.Summary,
            request.ContentMarkdown, request.CoverImageUrl, userId.Value, ct);
        return result.IsFailure ? result.Error.ToResponse() : this.ToOkResponse(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateBlogPostRequest request,
        CancellationToken ct)
    {
        logger.LogInformation("Admin updating blog post {Id}", id);
        var result = await service.UpdateAsync(
            id, request.Title, request.Summary, request.ContentMarkdown, request.CoverImageUrl, ct);
        return result.IsFailure ? result.Error.ToResponse() : this.ToOkResponse(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        logger.LogInformation("Admin deleting blog post {Id}", id);
        var result = await service.DeleteAsync(id, ct);
        return result.IsFailure ? result.Error.ToResponse() : this.ToOkResponse(result.Value);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
