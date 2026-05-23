using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Application.Blog;

// All blog mutations live in one service for brevity — v1 only has admin
// CRUD, so splitting into per-command service classes (like Volunteers/News)
// would be over-engineering. Revisit when the blog grows tags/categories/
// publishing workflow.
public class BlogPostsService(
    IBlogPostRepository repository,
    ILogger<BlogPostsService> logger)
{
    public async Task<Result<Guid, ErrorList>> CreateAsync(
        string slug, string language, string title, string summary,
        string contentMarkdown, string? coverImageUrl, Guid authorUserId,
        CancellationToken ct = default)
    {
        if (await repository.SlugExistsAsync(slug, ct))
            return new ErrorList([Error.Validation("blogpost.slug_taken", "Slug already exists")]);

        var post = BlogPost.Create(slug, language, title, summary, contentMarkdown, coverImageUrl, authorUserId);
        if (post.IsFailure)
            return new ErrorList([post.Error]);

        await repository.AddAsync(post.Value, ct);
        logger.LogInformation("BlogPost created. Id: {Id} Slug: {Slug}", post.Value.Id, post.Value.Slug);
        return post.Value.Id;
    }

    public async Task<Result<Guid, ErrorList>> UpdateAsync(
        Guid id, string title, string summary, string contentMarkdown, string? coverImageUrl,
        CancellationToken ct = default)
    {
        var post = await repository.GetByIdAsync(id, ct);
        if (post is null)
            return new ErrorList([Error.NotFound("blogpost.not_found", "Blog post not found")]);

        var update = post.Update(title, summary, contentMarkdown, coverImageUrl);
        if (update.IsFailure)
            return new ErrorList([update.Error]);

        await repository.SaveAsync(ct);
        logger.LogInformation("BlogPost updated. Id: {Id}", id);
        return id;
    }

    public async Task<Result<Guid, ErrorList>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await repository.GetByIdAsync(id, ct);
        if (post is null)
            return new ErrorList([Error.NotFound("blogpost.not_found", "Blog post not found")]);

        await repository.DeleteAsync(post, ct);
        logger.LogInformation("BlogPost deleted. Id: {Id}", id);
        return id;
    }
}
