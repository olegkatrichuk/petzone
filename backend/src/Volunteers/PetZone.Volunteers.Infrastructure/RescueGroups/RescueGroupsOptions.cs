namespace PetZone.Volunteers.Infrastructure.RescueGroups;

public class RescueGroupsOptions
{
    public const string SectionName = "RescueGroups";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.rescuegroups.org/v5/public/";
    public int PageSize { get; set; } = 100;
    public int MaxPages { get; set; } = 5;
}