using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;

namespace PetZone.Volunteers.Application.News;

public class UpdateNewsPostService(
    INewsPostRepository repository,
    ILogger<UpdateNewsPostService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateNewsPostCommand command,
        CancellationToken cancellationToken = default)
    {
        var newsPost = await repository.GetByIdAsync(command.NewsPostId, cancellationToken);
        if (newsPost is null)
            return new ErrorList([Error.NotFound("newspost.not_found", "News post not found")]);

        if (newsPost.VolunteerId != command.RequestingVolunteerId)
            return new ErrorList([Error.Forbidden("newspost.forbidden", "You can only edit your own posts")]);

        newsPost.Update(command.Title, command.Content);
        await repository.SaveAsync(cancellationToken);

        logger.LogInformation("NewsPost updated. Id: {NewsPostId}", newsPost.Id);
        return newsPost.Id;
    }
}