using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Photos",
                table: "pets",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photos",
                table: "pets");
        }
    }
}
