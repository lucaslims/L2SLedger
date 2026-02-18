using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2SLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona coluna type como nullable temporariamente
            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "public",
                table: "categories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Popula tipos para categorias de receita (Income)
            migrationBuilder.Sql(
                @"UPDATE public.categories 
                  SET type = 'Income' 
                  WHERE name IN ('Salário', 'Freelance', 'Investimentos');");

            // Popula tipos para todas as outras categorias (Expense)
            migrationBuilder.Sql(
                @"UPDATE public.categories 
                  SET type = 'Expense' 
                  WHERE type IS NULL;");

            // Torna a coluna NOT NULL após popular os dados
            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "public",
                table: "categories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            // Cria índice para performance de filtros por tipo
            migrationBuilder.CreateIndex(
                name: "idx_categories_type",
                schema: "public",
                table: "categories",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_categories_type",
                schema: "public",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "public",
                table: "categories");
        }
    }
}
