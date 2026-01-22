namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço de auditoria para registro automático de eventos.
/// Conforme ADR-014: auditoria é obrigatória e automática.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra um evento de criação de entidade.
    /// </summary>
    Task LogCreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Registra um evento de atualização de entidade.
    /// </summary>
    Task LogUpdateAsync<T>(T before, T after, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Registra um evento de exclusão de entidade.
    /// </summary>
    Task LogDeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Registra um evento de ajuste pós-fechamento.
    /// </summary>
    Task LogAdjustmentAsync(Guid adjustmentId, Guid originalTransactionId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um evento de fechamento de período.
    /// </summary>
    Task LogPeriodCloseAsync(Guid periodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um evento de reabertura de período.
    /// </summary>
    Task LogPeriodReopenAsync(Guid periodId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um evento de login bem-sucedido.
    /// </summary>
    Task LogLoginAsync(Guid userId, string email, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um evento de logout.
    /// </summary>
    Task LogLogoutAsync(Guid userId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma tentativa de login falha.
    /// </summary>
    Task LogLoginFailedAsync(string email, string? ipAddress, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um acesso negado (403).
    /// </summary>
    Task LogAccessDeniedAsync(Guid? userId, string resource, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma exportação de dados.
    /// </summary>
    Task LogExportAsync(Guid exportId, string exportType, CancellationToken cancellationToken = default);
}
