namespace PetZone.Volunteers.Domain.Models;

public class SystemNewsPost
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public DateTime PublishedAt { get; private set; }

    private SystemNewsPost() { }

    public static SystemNewsPost Create(string title, string content, string type) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Content = content.Trim(),
            Type = type,
            PublishedAt = DateTime.UtcNow,
        };
}