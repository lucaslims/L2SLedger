using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configuration;

/// <summary>
/// Configuração do Entity Framework Core para Adjustment.
/// </summary>
public class AdjustmentConfiguration : IEntityTypeConfiguration<Adjustment>
{
    public void Configure(EntityTypeBuilder<Adjustment> builder)
    {
        builder.ToTable("adjustments");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.OriginalTransactionId)
            .IsRequired();

        builder.Property(a => a.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.AdjustmentDate)
            .IsRequired();

        builder.Property(a => a.CreatedByUserId)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired(false);

        // Foreign Keys
        builder.HasOne(a => a.OriginalTransaction)
            .WithMany()
            .HasForeignKey(a => a.OriginalTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.OriginalTransactionId)
            .HasDatabaseName("IX_adjustments_original_transaction_id");

        builder.HasIndex(a => a.CreatedByUserId)
            .HasDatabaseName("IX_adjustments_created_by_user_id");

        builder.HasIndex(a => a.AdjustmentDate)
            .HasDatabaseName("IX_adjustments_adjustment_date");

        builder.HasIndex(a => a.Type)
            .HasDatabaseName("IX_adjustments_type");

        builder.HasIndex(a => new { a.OriginalTransactionId, a.AdjustmentDate })
            .HasDatabaseName("IX_adjustments_transaction_date");

        // Query Filter (soft delete)
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
