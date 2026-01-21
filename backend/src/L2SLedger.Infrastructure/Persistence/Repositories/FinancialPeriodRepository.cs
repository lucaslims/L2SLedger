using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório para operações de persistência da entidade FinancialPeriod.
/// ADR-034: PostgreSQL como fonte única de dados.
/// ADR-020: Clean Architecture - camada de infraestrutura.
/// </summary>
public class FinancialPeriodRepository : IFinancialPeriodRepository
{
    private readonly L2SLedgerDbContext _context;

    public FinancialPeriodRepository(L2SLedgerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Converte DateTime para UTC com Kind especificado.
    /// Requerido pelo Npgsql 6+ para colunas timestamp with time zone.
    /// </summary>
    private static DateTime ToUtcDate(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
    }

    /// <summary>
    /// Recupera um período financeiro por seu identificador único.
    /// </summary>
    public async Task<FinancialPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialPeriods
            .Include(p => p.ClosedByUser)
            .Include(p => p.ReopenedByUser)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Recupera um período financeiro por ano e mês.
    /// </summary>
    public async Task<FinancialPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialPeriods
            .FirstOrDefaultAsync(p => p.Year == year && p.Month == month, cancellationToken);
    }

    /// <summary>
    /// Recupera uma lista paginada de períodos financeiros com filtros opcionais.
    /// </summary>
    public async Task<(IEnumerable<FinancialPeriod> Periods, int TotalCount)> GetAllAsync(
        int? year,
        int? month,
        PeriodStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialPeriods.AsQueryable();

        // Aplicar filtros
        if (year.HasValue)
        {
            query = query.Where(p => p.Year == year.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(p => p.Month == month.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação (mais recentes primeiro)
        var periods = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (periods, totalCount);
    }

    /// <summary>
    /// Adiciona um novo período financeiro ao repositório.
    /// </summary>
    public async Task<FinancialPeriod> AddAsync(FinancialPeriod period, CancellationToken cancellationToken = default)
    {
        await _context.FinancialPeriods.AddAsync(period, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    /// <summary>
    /// Atualiza um período financeiro existente.
    /// </summary>
    public async Task UpdateAsync(FinancialPeriod period, CancellationToken cancellationToken = default)
    {
        _context.FinancialPeriods.Update(period);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Verifica se existe um período financeiro para o ano e mês informados.
    /// </summary>
    public async Task<bool> ExistsAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialPeriods
            .AnyAsync(p => p.Year == year && p.Month == month, cancellationToken);
    }

    /// <summary>
    /// Recupera o período financeiro que contém a data especificada.
    /// </summary>
    public async Task<FinancialPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateUtc = ToUtcDate(date);
        
        return await _context.FinancialPeriods
            .FirstOrDefaultAsync(
                p => dateUtc >= p.StartDate && dateUtc <= p.EndDate,
                cancellationToken);
    }
}
