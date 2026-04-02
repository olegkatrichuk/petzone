using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Listings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adoption_listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UserPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    BreedId = table.Column<Guid>(type: "uuid", nullable: true),
                    AgeMonths = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Vaccinated = table.Column<bool>(type: "boolean", nullable: false),
                    Castrated = table.Column<bool>(type: "boolean", nullable: false),
                    Photos = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adoption_listings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adoption_listings");
        }
    }
}
