using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_volunteers_is_deleted",
                table: "volunteers",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_volunteers_user_id",
                table: "volunteers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_is_deleted",
                table: "pets",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_pets_status",
                table: "pets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_pets_volunteer_id_is_deleted",
                table: "pets",
                columns: new[] { "volunteer_id", "is_deleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_volunteers_is_deleted",
                table: "volunteers");

            migrationBuilder.DropIndex(
                name: "IX_volunteers_user_id",
                table: "volunteers");

            migrationBuilder.DropIndex(
                name: "IX_pets_is_deleted",
                table: "pets");

            migrationBuilder.DropIndex(
                name: "IX_pets_status",
                table: "pets");

            migrationBuilder.DropIndex(
                name: "IX_pets_volunteer_id_is_deleted",
                table: "pets");
        }
    }
}
