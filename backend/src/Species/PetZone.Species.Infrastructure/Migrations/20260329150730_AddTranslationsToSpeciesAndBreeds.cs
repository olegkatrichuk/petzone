using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Species.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslationsToSpeciesAndBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "species");

            migrationBuilder.DropColumn(
                name: "name",
                table: "breeds");

            migrationBuilder.AddColumn<string>(
                name: "translations",
                table: "species",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "translations",
                table: "breeds",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "translations",
                table: "species");

            migrationBuilder.DropColumn(
                name: "translations",
                table: "breeds");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "species",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "breeds",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
