using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para a entidade Category.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        // Primary Key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Properties
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.ParentCategoryId)
            .HasColumnName("parent_category_id");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        // Relationships - Self-referencing para hierarquia
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("idx_categories_name");

        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("idx_categories_parent_id");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("idx_categories_is_active");

        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("idx_categories_is_deleted");

        // Unique constraint: Name dentro do mesmo ParentCategoryId (considerando soft delete)
        builder.HasIndex(c => new { c.Name, c.ParentCategoryId, c.IsDeleted })
            .HasDatabaseName("idx_categories_unique_name_per_parent")
            .IsUnique()
            .HasFilter("is_deleted = false");
    }
}
