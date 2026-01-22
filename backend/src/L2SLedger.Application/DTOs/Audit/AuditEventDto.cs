namespace L2SLedger.Application.DTOs.Audit;

/// <summary>
/// DTO de evento de auditoria para resposta da API.
/// </summary>
public class AuditEventDto
{
    /// <summary>ID do evento</summary>
    public Guid Id { get; set; }

    /// <summary>Tipo do evento (1=Create, 2=Update, etc.)</summary>
    public int EventType { get; set; }

    /// <summary>Nome do tipo de evento</summary>
    public string EventTypeName { get; set; } = string.Empty;

    /// <summary>Tipo da entidade afetada</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>ID da entidade afetada</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Estado antes da operação (JSON)</summary>
    public string? Before { get; set; }

    /// <summary>Estado depois da operação (JSON)</summary>
    public string? After { get; set; }

    /// <summary>ID do usuário responsável</summary>
    public Guid? UserId { get; set; }

    /// <summary>Email do usuário responsável</summary>
    public string? UserEmail { get; set; }

    /// <summary>Data e hora do evento (UTC)</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Origem da operação (1=UI, 2=API, etc.)</summary>
    public int Source { get; set; }

    /// <summary>Nome da origem</summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>IP de origem</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent</summary>
    public string? UserAgent { get; set; }

    /// <summary>Resultado da operação</summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>Detalhes ou mensagem de erro</summary>
    public string? Details { get; set; }

    /// <summary>TraceId para correlação</summary>
    public string? TraceId { get; set; }
}
