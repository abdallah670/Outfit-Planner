using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVoteUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_OptionId",
                table: "Votes");

            // Cleanup duplicate votes: keep only the most recent vote for each (OptionId, VoterId)
            migrationBuilder.Sql(@"
                DELETE FROM Votes 
                WHERE Id NOT IN (
                    SELECT MIN(Id) 
                    FROM Votes 
                    GROUP BY OptionId, VoterId
                )");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_OptionId_VoterId",
                table: "Votes",
                columns: new[] { "OptionId", "VoterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Votes_OptionId_VoterId",
                table: "Votes");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_OptionId",
                table: "Votes",
                column: "OptionId");
        }
    }
}
