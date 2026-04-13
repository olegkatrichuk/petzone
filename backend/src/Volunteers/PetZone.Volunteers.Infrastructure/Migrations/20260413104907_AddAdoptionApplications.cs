using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adoption_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    volunteer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicant_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    applicant_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adoption_applications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adoption_applications_applicant_user_id",
                table: "adoption_applications",
                column: "applicant_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_adoption_applications_pet_id_applicant_user_id",
                table: "adoption_applications",
                columns: new[] { "pet_id", "applicant_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_adoption_applications_volunteer_id",
                table: "adoption_applications",
                column: "volunteer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adoption_applications");
        }
    }
}
