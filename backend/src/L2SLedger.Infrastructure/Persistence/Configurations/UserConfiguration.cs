using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade User.
/// Conforme ADR-006 (PostgreSQL), ADR-010 (JSON para arrays).
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.FirebaseUid)
            .HasColumnName("firebase_uid")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(u => u.FirebaseUid)
            .IsUnique()
            .HasDatabaseName("ix_users_firebase_uid");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("ix_users_status");

        // Roles como JSON (ADR-010)
        builder.Property(u => u.Roles)
            .HasColumnName("roles")
            .HasColumnType("jsonb")
            .IsRequired();

        // Propriedades de Entity base
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("ix_users_is_deleted");

        // Query filter para soft delete (ADR-029)
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
