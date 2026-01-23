using AutoMapper;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Categories;

/// <summary>
/// Caso de uso para obter uma categoria por ID.
/// </summary>
public class GetCategoryByIdUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoryByIdUseCase(
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto> ExecuteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category == null)
        {
            throw new Domain.Exceptions.BusinessRuleException("FIN_CATEGORY_NOT_FOUND", "Categoria não encontrada");
        }

        return _mapper.Map<CategoryDto>(category);
    }
}
