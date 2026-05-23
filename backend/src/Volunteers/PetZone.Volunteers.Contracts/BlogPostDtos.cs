namespace PetZone.Volunteers.Contracts;

public record BlogPostDto(
    Guid Id,
    string Slug,
    string Language,
    string Title,
    string Summary,
    string ContentMarkdown,
    string? CoverImageUrl,
    Guid AuthorUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record BlogPostListItemDto(
    Guid Id,
    string Slug,
    string Language,
    string Title,
    string Summary,
    string? CoverImageUrl,
    DateTime CreatedAt);

public record PagedBlogPostsDto(
    IReadOnlyList<BlogPostListItemDto> Items,
    int Total,
    int Page,
    int PageSize);

public record CreateBlogPostRequest(
    string Slug,
    string Language,
    string Title,
    string Summary,
    string ContentMarkdown,
    string? CoverImageUrl);

public record UpdateBlogPostRequest(
    string Title,
    string Summary,
    string ContentMarkdown,
    string? CoverImageUrl);
