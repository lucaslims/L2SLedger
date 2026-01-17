using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2SLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adjustments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_adjustments_transactions_OriginalTransactionId",
                        column: x => x.OriginalTransactionId,
                        principalSchema: "public",
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_adjustments_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adjustments_adjustment_date",
                schema: "public",
                table: "adjustments",
                column: "AdjustmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_adjustments_created_by_user_id",
                schema: "public",
                table: "adjustments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_adjustments_original_transaction_id",
                schema: "public",
                table: "adjustments",
                column: "OriginalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_adjustments_transaction_date",
                schema: "public",
                table: "adjustments",
                columns: new[] { "OriginalTransactionId", "AdjustmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_adjustments_type",
                schema: "public",
                table: "adjustments",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adjustments",
                schema: "public");
        }
    }
}
