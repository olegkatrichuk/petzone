namespace PetZone.Volunteers.Infrastructure.Options;

public class SoftDeleteOptions
{
    public const string SectionName = "SoftDelete";

    public int RetentionDays { get; set; } = 30;
}