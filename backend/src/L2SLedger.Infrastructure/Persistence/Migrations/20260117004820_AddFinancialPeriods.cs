using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2SLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "financial_periods",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    closed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reopened_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reopened_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reopen_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    total_income = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    total_expense = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    net_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    balance_snapshot_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_periods_users_closed_by_user_id",
                        column: x => x.closed_by_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_financial_periods_users_reopened_by_user_id",
                        column: x => x.reopened_by_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_financial_periods_closed_by_user_id",
                schema: "public",
                table: "financial_periods",
                column: "closed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_financial_periods_reopened_by_user_id",
                schema: "public",
                table: "financial_periods",
                column: "reopened_by_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_financial_periods_status",
                schema: "public",
                table: "financial_periods",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_financial_periods_year_month_desc",
                schema: "public",
                table: "financial_periods",
                columns: new[] { "year", "month" },
                unique: true,
                descending: new bool[0],
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_periods",
                schema: "public");
        }
    }
}
