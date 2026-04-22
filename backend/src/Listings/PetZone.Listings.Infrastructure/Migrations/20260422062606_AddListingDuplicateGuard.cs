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
            // Soft-close duplicate active listings, keeping the oldest one per (UserId, Title).
            migrationBuilder.Sql("""
                UPDATE adoption_listings
                SET "Status" = 'Removed'
                WHERE "Id" IN (
                    SELECT "Id" FROM (
                        SELECT "Id",
                               ROW_NUMBER() OVER (
                                   PARTITION BY "UserId", LOWER("Title")
                                   ORDER BY "CreatedAt"
                               ) AS rn
                        FROM adoption_listings
                        WHERE "Status" = 'Active'
                    ) ranked
                    WHERE rn > 1
                );
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_adoption_listings_user_id_title_active"
                ON adoption_listings ("UserId", LOWER("Title"))
                WHERE "Status" = 'Active';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_adoption_listings_user_id_title_active";""");
        }
    }
}
