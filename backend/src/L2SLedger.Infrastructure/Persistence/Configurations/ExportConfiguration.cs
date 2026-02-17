using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

public class ExportConfiguration : IEntityTypeConfiguration<Export>
{
    public void Configure(EntityTypeBuilder<Export> builder)
    {
        builder.ToTable("exports");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExportType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Format)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.FilePath)
            .HasMaxLength(500);

        builder.Property(e => e.ParametersJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(e => e.RequestedByUser)
            .WithMany()
            .HasForeignKey(e => e.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        // Índices
        builder.HasIndex(e => e.RequestedByUserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestedAt);
        builder.HasIndex(e => new { e.Status, e.RequestedAt });

        // Query Filter para soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
