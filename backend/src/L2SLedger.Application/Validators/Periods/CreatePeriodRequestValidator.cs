using FluentValidation;
using L2SLedger.Application.DTOs.Periods;

namespace L2SLedger.Application.Validators.Periods;

/// <summary>
/// Validator para request de criação de período financeiro.
/// </summary>
public class CreatePeriodRequestValidator : AbstractValidator<CreatePeriodRequest>
{
    public CreatePeriodRequestValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Ano deve estar entre 2000 e 2100");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12");
    }
}
