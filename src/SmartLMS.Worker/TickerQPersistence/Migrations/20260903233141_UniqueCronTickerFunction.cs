using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLMS.Worker.TickerQPersistence.Migrations
{
    /// <inheritdoc />
    public partial class UniqueCronTickerFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_Function",
                schema: "ticker",
                table: "CronTickers",
                column: "Function",
                unique: true,
                filter: "[Function] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CronTickers_Function",
                schema: "ticker",
                table: "CronTickers");
        }
    }
}
