// Blog v1 lives inside the Volunteers module because the existing News pattern
// is similar (admin-authored content with markdown body, public read +
// authenticated write). When/if the blog grows beyond simple CRUD it can be
// extracted to its own module without touching the public API contract.

using CSharpFunctionalExtensions;
using PetZone.SharedKernel;

namespace PetZone.Volunteers.Domain.Models;

public class BlogPost
{
    public const int MaxSlugLength = 200;
    public const int MaxTitleLength = 200;
    public const int MaxSummaryLength = 500;
    public const int MaxContentLength = 50_000;
    public const int MaxLanguageLength = 8;
    public const int MaxCoverImageUrlLength = 500;

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string ContentMarkdown { get; private set; } = string.Empty;
    public string? CoverImageUrl { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private BlogPost() { }

    public static Result<BlogPost, Error> Create(
        string slug,
        string language,
        string title,
        string summary,
        string contentMarkdown,
        string? coverImageUrl,
        Guid authorUserId)
    {
        var validation = Validate(slug, language, title, summary, contentMarkdown, coverImageUrl);
        if (validation.IsFailure) return validation.Error;

        return new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = slug.Trim(),
            Language = language.Trim().ToLowerInvariant(),
            Title = title.Trim(),
            Summary = summary.Trim(),
            ContentMarkdown = contentMarkdown,
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim(),
            AuthorUserId = authorUserId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public UnitResult<Error> Update(
        string title,
        string summary,
        string contentMarkdown,
        string? coverImageUrl)
    {
        var validation = Validate(Slug, Language, title, summary, contentMarkdown, coverImageUrl);
        if (validation.IsFailure) return validation.Error;

        Title = title.Trim();
        Summary = summary.Trim();
        ContentMarkdown = contentMarkdown;
        CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
        return UnitResult.Success<Error>();
    }

    private static UnitResult<Error> Validate(
        string slug, string language, string title, string summary,
        string contentMarkdown, string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > MaxSlugLength)
            return Error.Validation("blogpost.invalid_slug", $"Slug must be 1..{MaxSlugLength} chars");
        if (!System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            return Error.Validation("blogpost.invalid_slug_format", "Slug must be lowercase, digits and single hyphens");
        if (string.IsNullOrWhiteSpace(language) || language.Length > MaxLanguageLength)
            return Error.Validation("blogpost.invalid_language", "Language is required");
        if (string.IsNullOrWhiteSpace(title) || title.Length > MaxTitleLength)
            return Error.Validation("blogpost.invalid_title", $"Title must be 1..{MaxTitleLength} chars");
        if (string.IsNullOrWhiteSpace(summary) || summary.Length > MaxSummaryLength)
            return Error.Validation("blogpost.invalid_summary", $"Summary must be 1..{MaxSummaryLength} chars");
        if (string.IsNullOrWhiteSpace(contentMarkdown) || contentMarkdown.Length > MaxContentLength)
            return Error.Validation("blogpost.invalid_content", $"Content must be 1..{MaxContentLength} chars");
        if (!string.IsNullOrEmpty(coverImageUrl) && coverImageUrl.Length > MaxCoverImageUrlLength)
            return Error.Validation("blogpost.invalid_cover_url", "Cover URL too long");
        return UnitResult.Success<Error>();
    }
}
