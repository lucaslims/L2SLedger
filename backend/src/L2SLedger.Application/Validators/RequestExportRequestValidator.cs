using FluentValidation;
using L2SLedger.Application.DTOs.Exports;

namespace L2SLedger.Application.Validators;

/// <summary>
/// Validador para RequestExportRequest.
/// </summary>
public class RequestExportRequestValidator : AbstractValidator<RequestExportRequest>
{
    /// <summary>
    /// Construtor default do validador com as regras prévias definidas.
    /// </summary>
    public RequestExportRequestValidator()
    {
        RuleFor(x => x.Format)
            .InclusiveBetween(1, 2)
            .WithMessage("Format must be 1 (CSV) or 2 (PDF).");

        RuleFor(x => x)
            .Must(x => x.StartDate == null || x.EndDate == null || x.StartDate <= x.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");

        RuleFor(x => x)
            .Must(x => x.StartDate == null || x.EndDate == null || (x.EndDate.Value - x.StartDate.Value).TotalDays <= 365)
            .WithMessage("Export period cannot exceed 365 days.");

        RuleFor(x => x.TransactionType)
            .InclusiveBetween(1, 2)
            .When(x => x.TransactionType.HasValue)
            .WithMessage("TransactionType must be 1 (Income) or 2 (Expense).");
    }
}
