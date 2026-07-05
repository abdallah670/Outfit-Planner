using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutfitPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSentReminderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SentReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CalendarEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReminderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentReminders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SentReminders_UserId",
                table: "SentReminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SentReminders_UserId_CalendarEventId_ReminderType_SentAt",
                table: "SentReminders",
                columns: new[] { "UserId", "CalendarEventId", "ReminderType", "SentAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentReminders");
        }
    }
}
