using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço para cálculo de snapshots de saldo para períodos financeiros.
/// Implementa ADR-015 (Snapshot para Imutabilidade) e ADR-020 (Clean Architecture).
/// </summary>
public interface IPeriodBalanceService
{
    /// <summary>
    /// Calcula um snapshot de saldo para um período financeiro específico.
    /// Agrega todas as transações por categoria e calcula os totais.
    /// </summary>
    /// <param name="year">O ano do período</param>
    /// <param name="month">O mês do período (1-12)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Um snapshot completo de saldo com saldos por categoria e totais</returns>
    Task<BalanceSnapshot> CalculateBalanceSnapshotAsync(
        int year, 
        int month, 
        CancellationToken cancellationToken = default);
}
