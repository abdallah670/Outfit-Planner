using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class changenameofFollowing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FollowingId",
                table: "Follows",
                newName: "FollowedId");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_FollowingId",
                table: "Follows",
                newName: "IX_Follows_FollowedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FollowedId",
                table: "Follows",
                newName: "FollowingId");

            migrationBuilder.RenameIndex(
                name: "IX_Follows_FollowedId",
                table: "Follows",
                newName: "IX_Follows_FollowingId");
        }
    }
}
