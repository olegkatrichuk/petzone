using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using PetZone.SharedKernel;
using PetZone.Volunteers.Application.Repositories;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Application.News;

public class CreateNewsPostService(
    INewsPostRepository repository,
    ILogger<CreateNewsPostService> logger)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        CreateNewsPostCommand command,
        CancellationToken cancellationToken = default)
    {
        var newsPost = NewsPost.Create(command.VolunteerId, command.Title, command.Content);
        await repository.AddAsync(newsPost, cancellationToken);

        logger.LogInformation("NewsPost created. Id: {NewsPostId}", newsPost.Id);
        return newsPost.Id;
    }
}