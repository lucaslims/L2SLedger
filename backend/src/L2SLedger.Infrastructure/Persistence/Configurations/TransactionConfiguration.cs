using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para Transaction.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.CategoryId)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        builder.Property(t => t.IsRecurring)
            .IsRequired();

        builder.Property(t => t.RecurringDay)
            .IsRequired(false);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);

        // Foreign Keys
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("IX_transactions_user_id");

        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_transactions_category_id");

        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("IX_transactions_transaction_date");

        builder.HasIndex(t => new { t.UserId, t.TransactionDate })
            .HasDatabaseName("IX_transactions_user_date");

        builder.HasIndex(t => new { t.UserId, t.IsDeleted })
            .HasDatabaseName("IX_transactions_user_deleted");

        // Query Filter (soft delete)
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
