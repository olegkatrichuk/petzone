using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateSeededVolunteers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Soft-delete pets that belong to duplicate volunteer records.
            // Duplicates = volunteers with the same email where we keep only
            // the one with the smallest id (first created).
            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT id,
                           ROW_NUMBER() OVER (PARTITION BY email ORDER BY id) AS rn
                    FROM volunteers
                    WHERE is_deleted = false AND is_system = false
                ),
                dupes AS (SELECT id FROM ranked WHERE rn > 1)
                UPDATE pets
                SET is_deleted = true
                WHERE volunteer_id IN (SELECT id FROM dupes);
                """);

            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT id,
                           ROW_NUMBER() OVER (PARTITION BY email ORDER BY id) AS rn
                    FROM volunteers
                    WHERE is_deleted = false AND is_system = false
                ),
                dupes AS (SELECT id FROM ranked WHERE rn > 1)
                UPDATE volunteers
                SET is_deleted = true
                WHERE id IN (SELECT id FROM dupes);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally not reversible — we cannot know which records were
            // duplicates after they have been soft-deleted.
        }
    }
}
