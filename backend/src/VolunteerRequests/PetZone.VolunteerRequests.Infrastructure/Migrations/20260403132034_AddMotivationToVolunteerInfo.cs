using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.VolunteerRequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivationToVolunteerInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motivation",
                schema: "volunteer_requests",
                table: "volunteer_requests",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "motivation",
                schema: "volunteer_requests",
                table: "volunteer_requests");
        }
    }
}
