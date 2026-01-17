using FluentValidation;
using L2SLedger.Application.DTOs.Adjustments;

namespace L2SLedger.Application.Validators.Adjustments;

/// <summary>
/// Validator para CreateAdjustmentRequest.
/// </summary>
public class CreateAdjustmentRequestValidator : AbstractValidator<CreateAdjustmentRequest>
{
    public CreateAdjustmentRequestValidator()
    {
        RuleFor(x => x.OriginalTransactionId)
            .NotEmpty()
            .WithMessage("OriginalTransactionId é obrigatório");

        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage("Amount não pode ser zero");

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 3)
            .WithMessage("Type deve ser 1 (Correction), 2 (Reversal) ou 3 (Compensation)");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Justificativa é obrigatória")
            .MinimumLength(10)
            .WithMessage("Justificativa deve ter no mínimo 10 caracteres")
            .MaximumLength(500)
            .WithMessage("Justificativa não pode exceder 500 caracteres");

        RuleFor(x => x.AdjustmentDate)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.AdjustmentDate.HasValue)
            .WithMessage("Data do ajuste não pode ser futura");
    }
}
