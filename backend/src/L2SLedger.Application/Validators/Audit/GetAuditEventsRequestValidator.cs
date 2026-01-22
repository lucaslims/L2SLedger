using FluentValidation;
using L2SLedger.Application.DTOs.Audit;

namespace L2SLedger.Application.Validators.Audit;

/// <summary>
/// Validator para GetAuditEventsRequest.
/// </summary>
public class GetAuditEventsRequestValidator : AbstractValidator<GetAuditEventsRequest>
{
    private static readonly string[] ValidResults = ["Success", "Failed", "Denied"];

    public GetAuditEventsRequestValidator()
    {
        RuleFor(x => x.EventType)
            .InclusiveBetween(1, 20)
            .When(x => x.EventType.HasValue)
            .WithMessage("EventType inválido");

        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityType))
            .WithMessage("EntityType deve ter no máximo 100 caracteres");

        RuleFor(x => x.Result)
            .Must(r => ValidResults.Contains(r))
            .When(x => !string.IsNullOrEmpty(x.Result))
            .WithMessage("Result deve ser 'Success', 'Failed' ou 'Denied'");

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
