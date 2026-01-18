using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework Core para o L2SLedger.
/// Conforme ADR-006 (PostgreSQL), ADR-020 (Clean Architecture).
/// </summary>
public class L2SLedgerDbContext : DbContext
{
    public L2SLedgerDbContext(DbContextOptions<L2SLedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();
    public DbSet<Adjustment> Adjustments => Set<Adjustment>();
    public DbSet<Export> Exports => Set<Export>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(L2SLedgerDbContext).Assembly);

        // Configuração global: usar schema "public" (padrão PostgreSQL)
        modelBuilder.HasDefaultSchema("public");
    }
}
