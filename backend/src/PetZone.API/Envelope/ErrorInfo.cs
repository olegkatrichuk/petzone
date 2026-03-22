using PetZone.SharedKernel;

namespace PetZone.API.Envelope;

public record ErrorInfo(string? ErrorCode, string? ErrorMessage, string? InvalidField)
{
    public static ErrorInfo FromError(Error error) =>
        new(error.Code, error.Description, error.InvalidField);
}
