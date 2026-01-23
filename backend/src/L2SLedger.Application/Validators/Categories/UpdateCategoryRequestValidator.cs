using FluentValidation;
using L2SLedger.Application.DTOs.Categories;

namespace L2SLedger.Application.Validators.Categories;

/// <summary>
/// Validador para atualização de categorias.
/// </summary>
public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da categoria é obrigatório")
            .WithErrorCode("VAL_REQUIRED_FIELD")
            .MaximumLength(100)
            .WithMessage("Nome da categoria não pode exceder 100 caracteres")
            .WithErrorCode("VAL_INVALID_FORMAT");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Descrição não pode exceder 500 caracteres")
            .WithErrorCode("VAL_INVALID_FORMAT")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
