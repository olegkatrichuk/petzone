namespace PetZone.Volunteers.Domain.Models;

public class NewsPost
{
    public Guid Id { get; private set; }
    public Guid VolunteerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private NewsPost() { }

    public static NewsPost Create(Guid volunteerId, string title, string content)
    {
        return new NewsPost
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            Title = title.Trim(),
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(string title, string content)
    {
        Title = title.Trim();
        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
