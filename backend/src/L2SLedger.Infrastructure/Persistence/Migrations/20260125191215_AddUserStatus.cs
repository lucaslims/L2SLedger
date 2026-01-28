using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2SLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "public",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Usuários existentes são automaticamente marcados como Active
            migrationBuilder.Sql(@"UPDATE public.users SET status = 1 WHERE status = 0;");

            migrationBuilder.CreateIndex(
                name: "ix_users_status",
                schema: "public",
                table: "users",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_status",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "public",
                table: "users");
        }
    }
}
