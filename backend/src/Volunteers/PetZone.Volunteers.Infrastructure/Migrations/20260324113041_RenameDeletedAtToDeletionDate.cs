using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeletedAtToDeletionDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "volunteers",
                newName: "deletion_date");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "pets",
                newName: "deletion_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deletion_date",
                table: "volunteers",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "deletion_date",
                table: "pets",
                newName: "deleted_at");
        }
    }
}
