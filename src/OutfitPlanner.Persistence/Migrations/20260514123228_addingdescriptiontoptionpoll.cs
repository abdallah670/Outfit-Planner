using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addingdescriptiontoptionpoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PollOptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PollOptions");
        }
    }
}
