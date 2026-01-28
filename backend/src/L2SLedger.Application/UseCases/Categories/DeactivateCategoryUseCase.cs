using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;

namespace L2SLedger.Application.UseCases.Categories;

/// <summary>
/// Caso de uso para desativar (soft delete) uma categoria.
/// </summary>
public class DeactivateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public DeactivateCategoryUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task ExecuteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        // Carregar categoria
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
        {
            throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.FIN_CATEGORY_NOT_FOUND, "Categoria não encontrada");
        }

        // Verificar se tem subcategorias ativas
        var activeSubCategoriesCount = await _categoryRepository.CountSubCategoriesAsync(categoryId, activeOnly: true, cancellationToken);
        if (activeSubCategoriesCount > 0)
        {
            throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.VAL_BUSINESS_RULE_VIOLATION, "Não é possível desativar uma categoria com subcategorias ativas");
        }

        // Desativar categoria (soft delete)
        category.Deactivate();

        // Salvar
        await _categoryRepository.UpdateAsync(category, cancellationToken);
    }
}
