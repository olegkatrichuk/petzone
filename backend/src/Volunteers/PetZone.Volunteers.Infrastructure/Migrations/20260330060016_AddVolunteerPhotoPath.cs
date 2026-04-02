using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerPhotoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "photo_path",
                table: "volunteers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "photo_path",
                table: "volunteers");
        }
    }
}
