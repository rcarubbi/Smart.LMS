using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UniqueClassAccessAndOutboxDrainIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassAccesses_UserId_ClassId",
                table: "ClassAccesses");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_Attempts_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "Attempts", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassAccesses_UserId_ClassId",
                table: "ClassAccesses",
                columns: new[] { "UserId", "ClassId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn_Attempts_CreatedAt",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_ClassAccesses_UserId_ClassId",
                table: "ClassAccesses");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAccesses_UserId_ClassId",
                table: "ClassAccesses",
                columns: new[] { "UserId", "ClassId" });
        }
    }
}
