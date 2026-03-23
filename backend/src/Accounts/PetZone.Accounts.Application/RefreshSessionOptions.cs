namespace PetZone.Accounts.Application;

public class RefreshSessionOptions
{
    public const string SectionName = "Jwt";
    public int RefreshExpirationDays { get; init; } = 7;
}