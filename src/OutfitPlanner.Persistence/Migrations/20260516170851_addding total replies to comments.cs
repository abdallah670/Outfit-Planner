using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class adddingtotalrepliestocomments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalReplies",
                table: "PostComments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalReplies",
                table: "PostComments");
        }
    }
}
