using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence.Seeds;

/// <summary>
/// Seeder de categorias padrão para ambientes DEV/DEMO.
/// Conforme ADR-029 (Estratégia de Seed de Dados Financeiros).
/// </summary>
public static class CategorySeeder
{
    /// <summary>
    /// Cria categorias padrão caso não existam categorias no banco.
    /// </summary>
    public static async Task SeedAsync(L2SLedgerDbContext context)
    {
        // Verifica se já existem categorias
        if (await context.Categories.AnyAsync())
        {
            return; // Já existem categorias, não precisa fazer seed
        }

        var categories = new List<Category>
        {
            // Receitas (categorias raiz)
            new Category("Salário", "Rendimentos de trabalho formal"),
            new Category("Freelance", "Trabalhos autônomos e projetos externos"),
            new Category("Investimentos", "Rendimentos de aplicações financeiras"),
            
            // Despesas (categorias raiz)
            new Category("Alimentação", "Gastos com alimentação (mercado, restaurantes, lanches)"),
            new Category("Transporte", "Gastos com deslocamento (combustível, transporte público, aplicativos)"),
            new Category("Moradia", "Aluguel, condomínio, IPTU, manutenção"),
            new Category("Saúde", "Planos de saúde, medicamentos, consultas médicas"),
            new Category("Lazer", "Entretenimento, diversão, viagens, hobbies")
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}
