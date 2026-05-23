using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sync_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_states", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sync_states_service_name",
                table: "sync_states",
                column: "service_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sync_states");
        }
    }
}
