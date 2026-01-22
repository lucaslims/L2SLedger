using System.Diagnostics;
using System.Text.Json;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Services;

/// <summary>
/// Serviço de auditoria para registro automático de eventos.
/// Conforme ADR-014: auditoria é obrigatória e automática.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(
        IAuditEventRepository auditEventRepository,
        ICurrentUserService currentUserService,
        ILogger<AuditService> logger)
    {
        _auditEventRepository = auditEventRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private static string? GetTraceId() => Activity.Current?.Id;

    private (Guid? UserId, string? Email) GetCurrentUser()
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var email = _currentUserService.GetUserEmail();
            return (userId, email);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task LogCreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        var (userId, email) = GetCurrentUser();
        var entityId = GetEntityId(entity);

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Create,
            typeof(T).Name,
            entityId,
            userId,
            email,
            AuditSource.API,
            before: null,
            after: SerializeEntity(entity),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogUpdateAsync<T>(T before, T after, CancellationToken cancellationToken = default) where T : class
    {
        var (userId, email) = GetCurrentUser();
        var entityId = GetEntityId(after);

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Update,
            typeof(T).Name,
            entityId,
            userId,
            email,
            AuditSource.API,
            before: SerializeEntity(before),
            after: SerializeEntity(after),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogDeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        var (userId, email) = GetCurrentUser();
        var entityId = GetEntityId(entity);

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Delete,
            typeof(T).Name,
            entityId,
            userId,
            email,
            AuditSource.API,
            before: SerializeEntity(entity),
            after: null,
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogAdjustmentAsync(Guid adjustmentId, Guid originalTransactionId, string reason, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Adjust,
            "Adjustment",
            adjustmentId,
            userId,
            email,
            AuditSource.API,
            after: JsonSerializer.Serialize(new { originalTransactionId, reason }, JsonOptions),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogPeriodCloseAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Close,
            "FinancialPeriod",
            periodId,
            userId,
            email,
            AuditSource.API,
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogPeriodReopenAsync(Guid periodId, string reason, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Reopen,
            "FinancialPeriod",
            periodId,
            userId,
            email,
            AuditSource.API,
            after: JsonSerializer.Serialize(new { reason }, JsonOptions),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogLoginAsync(Guid userId, string email, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.Login,
            userId,
            email,
            ipAddress,
            userAgent,
            "Success",
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogLogoutAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.Logout,
            userId,
            email,
            null,
            null,
            "Success",
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogLoginFailedAsync(string email, string? ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.LoginFailed,
            null,
            email,
            ipAddress,
            null,
            "Failed",
            details: reason,
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogAccessDeniedAsync(Guid? userId, string resource, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.AccessDenied,
            userId,
            null,
            ipAddress,
            null,
            "Denied",
            details: $"Acesso negado ao recurso: {resource}",
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogExportAsync(Guid exportId, string exportType, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Export,
            "Export",
            exportId,
            userId,
            email,
            AuditSource.API,
            after: JsonSerializer.Serialize(new { exportType }, JsonOptions),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    private async Task SaveEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _auditEventRepository.AddAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            // Auditoria não deve bloquear operações - log e continua
            _logger.LogError(ex, "Erro ao salvar evento de auditoria: {EventType} {EntityType}",
                auditEvent.EventType, auditEvent.EntityType);
        }
    }

    private static Guid GetEntityId<T>(T entity) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty?.GetValue(entity) is Guid id)
        {
            return id;
        }
        return Guid.Empty;
    }

    private static string? SerializeEntity<T>(T entity) where T : class
    {
        try
        {
            return JsonSerializer.Serialize(entity, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
