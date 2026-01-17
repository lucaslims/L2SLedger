using FluentValidation;
using L2SLedger.Application.DTOs.Adjustments;

namespace L2SLedger.Application.Validators.Adjustments;

/// <summary>
/// Validator para GetAdjustmentsRequest.
/// </summary>
public class GetAdjustmentsRequestValidator : AbstractValidator<GetAdjustmentsRequest>
{
    public GetAdjustmentsRequestValidator()
    {
        RuleFor(x => x.Type)
            .InclusiveBetween(1, 3)
            .When(x => x.Type.HasValue)
            .WithMessage("Type deve ser 1 (Correction), 2 (Reversal) ou 3 (Compensation)");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page deve ser maior que zero");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize deve estar entre 1 e 100");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Data inicial não pode ser posterior à data final");
    }
}
