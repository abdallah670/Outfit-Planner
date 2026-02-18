using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5765715a-93be-4628-86f7-b12e35a1a1f1", "ece01a6a-4caf-4a95-a704-9f03712e7fbb", "Admin", "ADMIN" },
                    { "76208571-0083-4a8b-9149-8d769c0d9c02", "bd9a512a-b188-467d-9fd9-875f09673ac3", "Planner", "PLANNER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5765715a-93be-4628-86f7-b12e35a1a1f1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "76208571-0083-4a8b-9149-8d769c0d9c02");
        }
    }
}
