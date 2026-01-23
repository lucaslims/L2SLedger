using FluentValidation;
using L2SLedger.Application.DTOs.Transaction;

namespace L2SLedger.Application.Validators.Transaction;

/// <summary>
/// Validator para UpdateTransactionRequest.
/// </summary>
public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Descrição é obrigatória")
            .MaximumLength(200)
            .WithMessage("Descrição deve ter no máximo 200 caracteres");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 2)
            .WithMessage("Tipo deve ser 1 (Receita) ou 2 (Despesa)");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Data da transação é obrigatória");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Observações devem ter no máximo 1000 caracteres");

        RuleFor(x => x.RecurringDay)
            .InclusiveBetween(1, 31)
            .When(x => x.IsRecurring && x.RecurringDay.HasValue)
            .WithMessage("Dia da recorrência deve estar entre 1 e 31");

        RuleFor(x => x)
            .Must(x => !x.IsRecurring || x.RecurringDay.HasValue)
            .WithMessage("Dia da recorrência é obrigatório para transações recorrentes");
    }
}
