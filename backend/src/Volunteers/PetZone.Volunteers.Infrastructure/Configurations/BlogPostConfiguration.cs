using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetZone.Volunteers.Domain.Models;

namespace PetZone.Volunteers.Infrastructure.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Slug).HasMaxLength(BlogPost.MaxSlugLength).IsRequired();
        builder.Property(p => p.Language).HasMaxLength(BlogPost.MaxLanguageLength).IsRequired();
        builder.Property(p => p.Title).HasMaxLength(BlogPost.MaxTitleLength).IsRequired();
        builder.Property(p => p.Summary).HasMaxLength(BlogPost.MaxSummaryLength).IsRequired();
        builder.Property(p => p.ContentMarkdown).HasMaxLength(BlogPost.MaxContentLength).IsRequired();
        builder.Property(p => p.CoverImageUrl).HasMaxLength(BlogPost.MaxCoverImageUrlLength);
        builder.Property(p => p.AuthorUserId).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => new { p.Language, p.CreatedAt });
    }
}
