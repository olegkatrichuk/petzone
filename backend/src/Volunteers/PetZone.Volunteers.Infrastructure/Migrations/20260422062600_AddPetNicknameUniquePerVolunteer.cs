using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetNicknameUniquePerVolunteer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Prevent manually-added pets with duplicate nickname per volunteer.
            // Applies only to locally-created pets (external_id IS NULL) that are not soft-deleted.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_pets_volunteer_id_nickname_manual"
                ON pets (volunteer_id, LOWER(nickname))
                WHERE external_id IS NULL AND is_deleted = FALSE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_pets_volunteer_id_nickname_manual";""");
        }
    }
}
