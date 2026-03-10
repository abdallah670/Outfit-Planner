using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrendingOutfits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutfitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteCount = table.Column<int>(type: "int", nullable: false),
                    ReactionCount = table.Column<int>(type: "int", nullable: false),
                    TrendingScore = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RankPosition = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendingOutfits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrendingOutfits_Outfits_OutfitId",
                        column: x => x.OutfitId,
                        principalTable: "Outfits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrendingOutfits_ValidationPolls_PollId",
                        column: x => x.PollId,
                        principalTable: "ValidationPolls",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoteReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReactionType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoteReactions_Votes_VoteId",
                        column: x => x.VoteId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrendingOutfits_Date_RankPosition",
                table: "TrendingOutfits",
                columns: new[] { "Date", "RankPosition" });

            migrationBuilder.CreateIndex(
                name: "IX_TrendingOutfits_OutfitId_Date",
                table: "TrendingOutfits",
                columns: new[] { "OutfitId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrendingOutfits_PollId",
                table: "TrendingOutfits",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteReactions_UserId",
                table: "VoteReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteReactions_VoteId_UserId",
                table: "VoteReactions",
                columns: new[] { "VoteId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrendingOutfits");

            migrationBuilder.DropTable(
                name: "VoteReactions");
        }
    }
}
