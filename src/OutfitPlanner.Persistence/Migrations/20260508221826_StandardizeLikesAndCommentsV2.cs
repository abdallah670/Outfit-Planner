using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeLikesAndCommentsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "FeedPosts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "FeedPosts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "FeedPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "FeedPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
