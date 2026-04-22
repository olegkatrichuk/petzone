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
            // Soft-delete duplicate manually-added pets, keeping the one with the lowest position.
            migrationBuilder.Sql("""
                UPDATE pets
                SET is_deleted = TRUE, deletion_date = NOW()
                WHERE id IN (
                    SELECT id FROM (
                        SELECT id,
                               ROW_NUMBER() OVER (
                                   PARTITION BY volunteer_id, LOWER(nickname)
                                   ORDER BY position
                               ) AS rn
                        FROM pets
                        WHERE external_id IS NULL AND is_deleted = FALSE
                    ) ranked
                    WHERE rn > 1
                );
                """);

            // Now safe to create the unique index.
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
