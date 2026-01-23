using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade FinancialPeriod.
/// ADR-006: PostgreSQL como banco de dados.
/// ADR-015: Períodos financeiros para garantir imutabilidade.
/// </summary>
public class FinancialPeriodConfiguration : IEntityTypeConfiguration<FinancialPeriod>
{
    public void Configure(EntityTypeBuilder<FinancialPeriod> builder)
    {
        builder.ToTable("financial_periods");

        // Primary Key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Properties
        builder.Property(p => p.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(p => p.Month)
            .HasColumnName("month")
            .IsRequired();

        builder.Property(p => p.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(p => p.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.ClosedAt)
            .HasColumnName("closed_at")
            .HasColumnType("timestamp without time zone");

        builder.Property(p => p.ClosedByUserId)
            .HasColumnName("closed_by_user_id");

        builder.Property(p => p.ReopenedAt)
            .HasColumnName("reopened_at")
            .HasColumnType("timestamp without time zone");

        builder.Property(p => p.ReopenedByUserId)
            .HasColumnName("reopened_by_user_id");

        builder.Property(p => p.ReopenReason)
            .HasColumnName("reopen_reason")
            .HasMaxLength(500);

        builder.Property(p => p.TotalIncome)
            .HasColumnName("total_income")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.TotalExpense)
            .HasColumnName("total_expense")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.NetBalance)
            .HasColumnName("net_balance")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.BalanceSnapshotJson)
            .HasColumnName("balance_snapshot_json")
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.ClosedByUser)
            .WithMany()
            .HasForeignKey(p => p.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ReopenedByUser)
            .WithMany()
            .HasForeignKey(p => p.ReopenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // Único: year + month (quando não deletado)
        builder.HasIndex(p => new { p.Year, p.Month })
            .HasDatabaseName("idx_financial_periods_year_month_unique")
            .IsUnique()
            .HasFilter("is_deleted = false");

        // Ordenação padrão (mais recentes primeiro)
        builder.HasIndex(p => new { p.Year, p.Month })
            .HasDatabaseName("idx_financial_periods_year_month_desc")
            .IsDescending(true, true);

        // Filtro por status
        builder.HasIndex(p => p.Status)
            .HasDatabaseName("idx_financial_periods_status");

        // Auditoria de fechamento
        builder.HasIndex(p => p.ClosedByUserId)
            .HasDatabaseName("idx_financial_periods_closed_by_user_id");

        // Auditoria de reabertura
        builder.HasIndex(p => p.ReopenedByUserId)
            .HasDatabaseName("idx_financial_periods_reopened_by_user_id");

        // Query Filter (soft delete)
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
