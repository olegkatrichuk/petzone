using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestructureSystemNewsPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "title",
                table: "system_news_posts");

            migrationBuilder.AddColumn<string>(
                name: "fact_en",
                table: "system_news_posts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "found_home_this_week",
                table: "system_news_posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "looking_for_home",
                table: "system_news_posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "needs_help",
                table: "system_news_posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_volunteers",
                table: "system_news_posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fact_en",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "found_home_this_week",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "looking_for_home",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "needs_help",
                table: "system_news_posts");

            migrationBuilder.DropColumn(
                name: "total_volunteers",
                table: "system_news_posts");

            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "system_news_posts",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "system_news_posts",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }
    }
}
