using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDigestEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "featured_pet_breed",
                table: "system_news_posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "featured_pet_city",
                table: "system_news_posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "featured_pet_description",
                table: "system_news_posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "featured_pet_nickname",
                table: "system_news_posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "featured_pet_photo_url",
                table: "system_news_posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "top_breeds_json",
                table: "system_news_posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "top_city",
                table: "system_news_posts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "featured_pet_breed",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "featured_pet_city",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "featured_pet_description",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "featured_pet_nickname",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "featured_pet_photo_url",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "top_breeds_json",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "top_city",
                table: "system_news_posts");
        }
    }
}
