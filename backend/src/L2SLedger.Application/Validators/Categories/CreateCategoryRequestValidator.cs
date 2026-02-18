using FluentValidation;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Enums;

namespace L2SLedger.Application.Validators.Categories;

/// <summary>
/// Validador para criação de categorias.
/// </summary>
public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryRequestValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da categoria é obrigatório")
            .WithErrorCode("VAL_REQUIRED_FIELD")
            .MaximumLength(100)
            .WithMessage("Nome da categoria não pode exceder 100 caracteres")
            .WithErrorCode("VAL_INVALID_FORMAT")
            .MustAsync(BeUniqueName)
            .WithMessage("Já existe uma categoria com este nome")
            .WithErrorCode("VAL_DUPLICATE_NAME");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Tipo da categoria é obrigatório")
            .WithErrorCode("VAL_REQUIRED_FIELD")
            .Must(BeValidCategoryType)
            .WithMessage("Tipo de categoria inválido. Valores permitidos: Income, Expense")
            .WithErrorCode("FIN_CATEGORY_INVALID_TYPE")
            .When(x => !x.ParentCategoryId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Descrição não pode exceder 500 caracteres")
            .WithErrorCode("VAL_INVALID_FORMAT")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ParentCategoryId)
            .MustAsync(ParentCategoryMustExist)
            .WithMessage("Categoria pai não encontrada")
            .WithErrorCode("VAL_INVALID_REFERENCE")
            .MustAsync(ParentCategoryMustBeRoot)
            .WithMessage("Apenas categorias raiz podem ter subcategorias. Hierarquia máxima: 2 níveis")
            .WithErrorCode("VAL_BUSINESS_RULE_VIOLATION")
            .When(x => x.ParentCategoryId.HasValue);
    }

    private static bool BeValidCategoryType(string type)
    {
        return Enum.TryParse<CategoryType>(type, ignoreCase: true, out _);
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _categoryRepository.ExistsAsync(name, null, cancellationToken);
    }

    private async Task<bool> ParentCategoryMustExist(Guid? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
            return true;

        return await _categoryRepository.ExistsAsync(parentId.Value, cancellationToken);
    }

    private async Task<bool> ParentCategoryMustBeRoot(Guid? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
            return true;

        var parentCategory = await _categoryRepository.GetByIdAsync(parentId.Value, cancellationToken);
        return parentCategory?.IsRootCategory() ?? false;
    }
}
