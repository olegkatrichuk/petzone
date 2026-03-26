using CSharpFunctionalExtensions;
using PetZone.SharedKernel;
using PetZone.VolunteerRequests.Application.Repositories;

namespace PetZone.VolunteerRequests.Application.Commands.DeleteMessage;

public class DeleteMessageHandler(
    IDiscussionRepository repository,
    IVolunteerRequestsUnitOfWork unitOfWork)
{
    public async Task<Result<Guid, ErrorList>> Handle(
        DeleteMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var discussion = await repository.GetByIdAsync(command.DiscussionId, cancellationToken);
        if (discussion is null)
            return (ErrorList)Error.NotFound("discussion.not_found",
                $"Discussion {command.DiscussionId} not found.");

        var result = discussion.DeleteMessage(command.UserId, command.MessageId);
        if (result.IsFailure)
            return (ErrorList)result.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return command.MessageId;
    }
}