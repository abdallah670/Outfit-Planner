using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutfitEngagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutfitComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutfitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutfitComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutfitComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OutfitComments_OutfitComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "OutfitComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OutfitComments_Outfits_OutfitId",
                        column: x => x.OutfitId,
                        principalTable: "Outfits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutfitLikes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OutfitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutfitLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutfitLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OutfitLikes_Outfits_OutfitId",
                        column: x => x.OutfitId,
                        principalTable: "Outfits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutfitComments_OutfitId",
                table: "OutfitComments",
                column: "OutfitId");

            migrationBuilder.CreateIndex(
                name: "IX_OutfitComments_ParentCommentId",
                table: "OutfitComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutfitComments_UserId",
                table: "OutfitComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutfitLikes_OutfitId_UserId",
                table: "OutfitLikes",
                columns: new[] { "OutfitId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutfitLikes_UserId",
                table: "OutfitLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutfitComments");

            migrationBuilder.DropTable(
                name: "OutfitLikes");
        }
    }
}
