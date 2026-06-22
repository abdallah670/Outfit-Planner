using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTrendingOutfitsToFeedPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM TrendingOutfits;");

            migrationBuilder.DropForeignKey(
                name: "FK_TrendingOutfits_Outfits_OutfitId",
                table: "TrendingOutfits");

            migrationBuilder.DropForeignKey(
                name: "FK_TrendingOutfits_ValidationPolls_PollId",
                table: "TrendingOutfits");

            migrationBuilder.DropIndex(
                name: "IX_TrendingOutfits_PollId",
                table: "TrendingOutfits");

            migrationBuilder.DropColumn(
                name: "PollId",
                table: "TrendingOutfits");

            migrationBuilder.RenameColumn(
                name: "OutfitId",
                table: "TrendingOutfits",
                newName: "FeedPostId");

            migrationBuilder.RenameIndex(
                name: "IX_TrendingOutfits_OutfitId_Date",
                table: "TrendingOutfits",
                newName: "IX_TrendingOutfits_FeedPostId_Date");

            migrationBuilder.AddColumn<int>(
                name: "PostType",
                table: "TrendingOutfits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TrendingOutfits_FeedPosts_FeedPostId",
                table: "TrendingOutfits",
                column: "FeedPostId",
                principalTable: "FeedPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrendingOutfits_FeedPosts_FeedPostId",
                table: "TrendingOutfits");

            migrationBuilder.DropColumn(
                name: "PostType",
                table: "TrendingOutfits");

            migrationBuilder.RenameColumn(
                name: "FeedPostId",
                table: "TrendingOutfits",
                newName: "OutfitId");

            migrationBuilder.RenameIndex(
                name: "IX_TrendingOutfits_FeedPostId_Date",
                table: "TrendingOutfits",
                newName: "IX_TrendingOutfits_OutfitId_Date");

            migrationBuilder.AddColumn<Guid>(
                name: "PollId",
                table: "TrendingOutfits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrendingOutfits_PollId",
                table: "TrendingOutfits",
                column: "PollId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrendingOutfits_Outfits_OutfitId",
                table: "TrendingOutfits",
                column: "OutfitId",
                principalTable: "Outfits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrendingOutfits_ValidationPolls_PollId",
                table: "TrendingOutfits",
                column: "PollId",
                principalTable: "ValidationPolls",
                principalColumn: "Id");
        }
    }
}
