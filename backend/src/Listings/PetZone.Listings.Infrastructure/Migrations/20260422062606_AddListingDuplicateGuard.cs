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
            // Prevent a user from creating two active listings with the same title (case-insensitive).
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_adoption_listings_user_id_title_active"
                ON adoption_listings (user_id, LOWER(title))
                WHERE status = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_adoption_listings_user_id_title_active";""");
        }
    }
}
