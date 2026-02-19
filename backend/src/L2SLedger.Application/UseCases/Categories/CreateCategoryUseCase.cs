using AutoMapper;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Enums;
using FluentValidation;
using FluentValidationException = FluentValidation.ValidationException;

namespace L2SLedger.Application.UseCases.Categories;

/// <summary>
/// Caso de uso para criar uma nova categoria.
/// </summary>
public class CreateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCategoryRequest> _validator;

    public CreateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IValidator<CreateCategoryRequest> validator)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<CategoryDto> ExecuteAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        // Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidationException(validationResult.Errors);
        }

        // Verificar unicidade do nome dentro do mesmo escopo (mesmo pai)
        var nameExists = await _categoryRepository.ExistsAsync(request.Name, request.ParentCategoryId, cancellationToken);
        if (nameExists)
        {
            throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.VAL_DUPLICATE_NAME, "Já existe uma categoria com este nome no mesmo nível hierárquico");
        }

        // Se tiver pai, validar hierarquia e herdar tipo
        CategoryType categoryType;

        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.VAL_INVALID_REFERENCE, "Categoria pai não encontrada");
            }

            if (!parentCategory.CanHaveSubCategories())
            {
                throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.VAL_BUSINESS_RULE_VIOLATION, "Apenas categorias raiz podem ter subcategorias. Hierarquia máxima: 2 níveis");
            }

            // Subcategoria herda tipo do pai (ADR-044)
            categoryType = parentCategory.Type;
        }
        else
        {
            // Categoria raiz: parsear tipo do request
            if (!Enum.TryParse<CategoryType>(request.Type, ignoreCase: true, out categoryType))
            {
                throw new Domain.Exceptions.BusinessRuleException(ErrorCodes.FIN_CATEGORY_INVALID_TYPE, "Tipo de categoria inválido. Valores permitidos: Income, Expense");
            }
        }

        // Criar entidade
        var category = new Category(
            request.Name,
            categoryType,
            request.Description,
            request.ParentCategoryId
        );

        // Salvar
        await _categoryRepository.AddAsync(category, cancellationToken);

        // Retornar DTO
        return _mapper.Map<CategoryDto>(category);
    }
}
