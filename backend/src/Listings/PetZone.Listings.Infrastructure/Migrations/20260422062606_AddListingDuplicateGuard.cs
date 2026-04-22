using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Listings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingDuplicateGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Soft-close duplicate active listings, keeping the oldest one per (user_id, title).
            migrationBuilder.Sql("""
                UPDATE adoption_listings
                SET status = 'Removed'
                WHERE id IN (
                    SELECT id FROM (
                        SELECT id,
                               ROW_NUMBER() OVER (
                                   PARTITION BY user_id, LOWER(title)
                                   ORDER BY created_at
                               ) AS rn
                        FROM adoption_listings
                        WHERE status = 'Active'
                    ) ranked
                    WHERE rn > 1
                );
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_adoption_listings_user_id_title_active"
                ON adoption_listings (user_id, LOWER(title))
                WHERE status = 'Active';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_adoption_listings_user_id_title_active";""");
        }
    }
}
