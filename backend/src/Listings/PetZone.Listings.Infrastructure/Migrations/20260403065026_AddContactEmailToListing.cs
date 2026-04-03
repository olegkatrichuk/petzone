using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Listings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactEmailToListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "adoption_listings",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "adoption_listings");
        }
    }
}
