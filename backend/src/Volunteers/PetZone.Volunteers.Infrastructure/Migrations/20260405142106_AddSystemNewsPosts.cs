using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetZone.Volunteers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemNewsPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_news_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_news_posts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_news_posts_published_at",
                table: "system_news_posts",
                column: "published_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_news_posts");
        }
    }
}
