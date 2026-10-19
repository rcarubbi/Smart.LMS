using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxLeasesAndRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn_Attempts_CreatedAt",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId",
                table: "OutboxMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaseUntilUtc",
                table: "OutboxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_Attempts_NextAttemptAtUtc_LeaseUntilUtc_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "Attempts", "NextAttemptAtUtc", "LeaseUntilUtc", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOn_Attempts_NextAttemptAtUtc_LeaseUntilUtc_CreatedAt",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "ClaimId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LeaseUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtUtc",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_Attempts_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "Attempts", "CreatedAt" });
        }
    }
}
