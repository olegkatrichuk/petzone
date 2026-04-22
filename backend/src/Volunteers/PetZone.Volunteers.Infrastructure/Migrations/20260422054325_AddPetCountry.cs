using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "pets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pets_country",
                table: "pets",
                column: "country");

            // Backfill existing imported pets based on ExternalId prefix
            migrationBuilder.Sql("""
                UPDATE pets
                SET country = CASE
                    WHEN external_id LIKE 'pl:%'  THEN 'pl'
                    WHEN external_id LIKE 'de:%'  THEN 'de'
                    WHEN external_id LIKE 'fr:%'  THEN 'fr'
                    WHEN external_id IS NOT NULL   THEN 'ua'
                    ELSE NULL
                END
                WHERE country IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pets_country",
                table: "pets");

            migrationBuilder.DropColumn(
                name: "country",
                table: "pets");
        }
    }
}
