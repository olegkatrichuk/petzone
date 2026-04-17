using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Listings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable trigram extension for efficient ILIKE '%text%' searches
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // B-tree indexes for equality/range filters
            migrationBuilder.CreateIndex(
                name: "IX_adoption_listings_status",
                table: "adoption_listings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_adoption_listings_species_id",
                table: "adoption_listings",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_adoption_listings_created_at",
                table: "adoption_listings",
                column: "CreatedAt",
                descending: [true]);

            // Composite covering index for the most common query: active listings by species, newest first
            migrationBuilder.CreateIndex(
                name: "IX_adoption_listings_status_species_created",
                table: "adoption_listings",
                columns: ["Status", "SpeciesId", "CreatedAt"],
                descending: [false, false, true]);

            // GIN trigram indexes for ILIKE '%text%' searches
            migrationBuilder.Sql(@"
                CREATE INDEX IX_adoption_listings_city_trgm
                ON adoption_listings USING GIN (""City"" gin_trgm_ops);

                CREATE INDEX IX_adoption_listings_title_trgm
                ON adoption_listings USING GIN (""Title"" gin_trgm_ops);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_adoption_listings_status", "adoption_listings");
            migrationBuilder.DropIndex("IX_adoption_listings_species_id", "adoption_listings");
            migrationBuilder.DropIndex("IX_adoption_listings_created_at", "adoption_listings");
            migrationBuilder.DropIndex("IX_adoption_listings_status_species_created", "adoption_listings");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS IX_adoption_listings_city_trgm;
                DROP INDEX IF EXISTS IX_adoption_listings_title_trgm;
            ");
        }
    }
}
