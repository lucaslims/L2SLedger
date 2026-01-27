using AutoMapper;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using FluentValidation;
using FluentValidationException = FluentValidation.ValidationException;

namespace L2SLedger.Application.UseCases.Categories;

/// <summary>
/// Caso de uso para atualizar uma categoria existente.
/// </summary>
public class UpdateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateCategoryRequest> _validator;

    public UpdateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IValidator<UpdateCategoryRequest> validator)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<CategoryDto> ExecuteAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidationException(validationResult.Errors);
        }

        // Carregar categoria
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
        {
            throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.FIN_CATEGORY_NOT_FOUND, "Categoria não encontrada");
        }

        // Verificar unicidade do nome (excluindo a própria categoria)
        var nameExists = await _categoryRepository.ExistsAsync(request.Name, category.ParentCategoryId, cancellationToken);
        var existingCategory = nameExists 
            ? (await _categoryRepository.GetAllAsync(includeInactive: true, cancellationToken: cancellationToken))
                .FirstOrDefault(c => c.Name == request.Name && c.ParentCategoryId == category.ParentCategoryId)
            : null;

        if (existingCategory != null && existingCategory.Id != categoryId)
        {
            throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.VAL_DUPLICATE_NAME, "Já existe outra categoria com este nome no mesmo nível hierárquico");
        }

        // Atualizar dados
        category.UpdateName(request.Name);
        category.UpdateDescription(request.Description);

        // Salvar
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        // Retornar DTO
        return _mapper.Map<CategoryDto>(category);
    }
}
