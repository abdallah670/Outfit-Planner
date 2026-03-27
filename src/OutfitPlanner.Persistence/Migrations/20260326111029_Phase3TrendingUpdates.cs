using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase3TrendingUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PollId",
                table: "TrendingOutfits",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: "TrendingOutfits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "TrendingOutfits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrendingOutfits_Date_TrendingScore",
                table: "TrendingOutfits",
                columns: new[] { "Date", "TrendingScore" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrendingOutfits_Date_TrendingScore",
                table: "TrendingOutfits");

            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: "TrendingOutfits");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "TrendingOutfits");

            migrationBuilder.AlterColumn<Guid>(
                name: "PollId",
                table: "TrendingOutfits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
