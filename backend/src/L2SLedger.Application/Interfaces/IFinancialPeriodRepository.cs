using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface de repositório para o agregado FinancialPeriod.
/// Segue ADR-034 (Repository Pattern) e ADR-020 (Clean Architecture).
/// </summary>
public interface IFinancialPeriodRepository
{
    /// <summary>
    /// Recupera um período financeiro por seu identificador único.
    /// </summary>
    Task<FinancialPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um período financeiro por ano e mês.
    /// </summary>
    Task<FinancialPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera uma lista paginada de períodos financeiros com filtros opcionais.
    /// </summary>
    Task<(IEnumerable<FinancialPeriod> Periods, int TotalCount)> GetAllAsync(
        int? year,
        int? month,
        PeriodStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo período financeiro ao repositório.
    /// </summary>
    Task<FinancialPeriod> AddAsync(FinancialPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um período financeiro existente.
    /// </summary>
    Task UpdateAsync(FinancialPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe um período financeiro para o ano e mês informados.
    /// </summary>
    Task<bool> ExistsAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera o período financeiro que contém a data especificada.
    /// </summary>
    Task<FinancialPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default);
}
