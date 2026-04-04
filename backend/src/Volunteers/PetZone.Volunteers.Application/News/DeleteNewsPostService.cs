using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;

namespace PetZone.Volunteers.Application.News;

public class DeleteNewsPostService(
    INewsPostRepository repository,
    ILogger<DeleteNewsPostService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteNewsPostCommand command,
        CancellationToken cancellationToken = default)
    {
        var newsPost = await repository.GetByIdAsync(command.NewsPostId, cancellationToken);
        if (newsPost is null)
            return new ErrorList([Error.NotFound("newspost.not_found", "News post not found")]);

        if (newsPost.VolunteerId != command.RequestingVolunteerId)
            return new ErrorList([Error.Forbidden("newspost.forbidden", "You can only delete your own posts")]);

        await repository.DeleteAsync(newsPost, cancellationToken);

        logger.LogInformation("NewsPost deleted. Id: {NewsPostId}", newsPost.Id);
        return newsPost.Id;
    }
}