namespace PetZone.Infrastructure.Options;

public class SoftDeleteOptions
{
    public const string SectionName = "SoftDelete";
    
    // По умолчанию 30 дней
    public int RetentionDays { get; set; } = 30;
}