using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2SLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ParametersJson = table.Column<string>(type: "jsonb", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordCount = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exports_users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exports_RequestedAt",
                schema: "public",
                table: "exports",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_exports_RequestedByUserId",
                schema: "public",
                table: "exports",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_exports_Status",
                schema: "public",
                table: "exports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_exports_Status_RequestedAt",
                schema: "public",
                table: "exports",
                columns: new[] { "Status", "RequestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exports",
                schema: "public");
        }
    }
}
