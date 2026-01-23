using FluentValidation;
using L2SLedger.Application.DTOs.Periods;

namespace L2SLedger.Application.Validators.Periods;

/// <summary>
/// Validator para request de reabertura de período financeiro.
/// </summary>
public class ReopenPeriodRequestValidator : AbstractValidator<ReopenPeriodRequest>
{
    public ReopenPeriodRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Justificativa é obrigatória")
            .MinimumLength(10).WithMessage("Justificativa deve ter pelo menos 10 caracteres")
            .MaximumLength(500).WithMessage("Justificativa não pode exceder 500 caracteres");
    }
}
