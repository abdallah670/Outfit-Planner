using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialLoginFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "AspNetUsers");
        }
    }
}
