using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para AuditEvent.
/// NOTA: Esta tabela é IMUTÁVEL - sem UPDATE ou DELETE.
/// Conforme ADR-014 (Auditoria Financeira) e ADR-019 (Auditoria de Acessos).
/// </summary>
public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        // Primary Key
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Properties
        builder.Property(a => a.EventType)
            .HasColumnName("event_type")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .IsRequired(false);

        builder.Property(a => a.Before)
            .HasColumnName("before")
            .IsRequired(false)
            .HasColumnType("jsonb"); // PostgreSQL JSONB para performance

        builder.Property(a => a.After)
            .HasColumnName("after")
            .IsRequired(false)
            .HasColumnType("jsonb");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired(false);

        builder.Property(a => a.UserEmail)
            .HasColumnName("user_email")
            .IsRequired(false)
            .HasMaxLength(255);

        builder.Property(a => a.Timestamp)
            .HasColumnName("timestamp")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.Source)
            .HasColumnName("source")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .IsRequired(false)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(a => a.Result)
            .HasColumnName("result")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Details)
            .HasColumnName("details")
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(a => a.TraceId)
            .HasColumnName("trace_id")
            .IsRequired(false)
            .HasMaxLength(100);

        // Indexes para consultas frequentes
        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("ix_audit_events_timestamp")
            .IsDescending(); // Mais recentes primeiro

        builder.HasIndex(a => a.EventType)
            .HasDatabaseName("ix_audit_events_event_type");

        builder.HasIndex(a => a.EntityType)
            .HasDatabaseName("ix_audit_events_entity_type");

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("ix_audit_events_entity");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_audit_events_user_id");

        builder.HasIndex(a => new { a.Timestamp, a.EventType })
            .HasDatabaseName("ix_audit_events_timestamp_type");

        // NOTA: Não há QueryFilter pois eventos nunca são deletados
    }
}
