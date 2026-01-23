namespace L2SLedger.Domain.Entities;

/// <summary>
/// Evento de auditoria conforme ADR-014 e ADR-019.
/// Eventos são IMUTÁVEIS - apenas INSERT e SELECT são permitidos.
/// </summary>
public class AuditEvent
{
    /// <summary>Identificador único do evento</summary>
    public Guid Id { get; private set; }

    /// <summary>Tipo da operação (Create, Update, Delete, Login, etc.)</summary>
    public AuditEventType EventType { get; private set; }

    /// <summary>Tipo da entidade afetada (Transaction, Category, User, etc.)</summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>ID da entidade afetada (pode ser null para eventos de acesso)</summary>
    public Guid? EntityId { get; private set; }

    /// <summary>Estado ANTES da operação (JSON serializado)</summary>
    public string? Before { get; private set; }

    /// <summary>Estado DEPOIS da operação (JSON serializado)</summary>
    public string? After { get; private set; }

    /// <summary>ID do usuário que realizou a operação</summary>
    public Guid? UserId { get; private set; }

    /// <summary>Email do usuário (snapshot no momento do evento)</summary>
    public string? UserEmail { get; private set; }

    /// <summary>Data e hora do evento (UTC)</summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>Origem da operação (UI, API, Import, etc.)</summary>
    public AuditSource Source { get; private set; }

    /// <summary>IP de origem (para eventos de acesso)</summary>
    public string? IpAddress { get; private set; }

    /// <summary>User-Agent do cliente</summary>
    public string? UserAgent { get; private set; }

    /// <summary>Resultado da operação (Success, Failed, Denied)</summary>
    public string Result { get; private set; } = "Success";

    /// <summary>Detalhes adicionais ou mensagem de erro</summary>
    public string? Details { get; private set; }

    /// <summary>TraceId para correlação com logs técnicos</summary>
    public string? TraceId { get; private set; }

    // Construtor privado para EF Core
    private AuditEvent() { }

    /// <summary>
    /// Cria um novo evento de auditoria para operações em entidades.
    /// </summary>
    public static AuditEvent CreateEntityEvent(
        AuditEventType eventType,
        string entityType,
        Guid entityId,
        Guid? userId,
        string? userEmail,
        AuditSource source,
        string? before = null,
        string? after = null,
        string? traceId = null)
    {
        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            Source = source,
            Before = before,
            After = after,
            TraceId = traceId,
            Result = "Success"
        };
    }

    /// <summary>
    /// Cria um novo evento de auditoria para operações de acesso (login/logout).
    /// </summary>
    public static AuditEvent CreateAccessEvent(
        AuditEventType eventType,
        Guid? userId,
        string? userEmail,
        string? ipAddress,
        string? userAgent,
        string result,
        string? details = null,
        string? traceId = null)
    {
        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EntityType = "Access",
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            Source = AuditSource.API,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Result = result,
            Details = details,
            TraceId = traceId
        };
    }
}
