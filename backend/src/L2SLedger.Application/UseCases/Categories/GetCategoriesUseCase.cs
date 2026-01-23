using AutoMapper;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Categories;

/// <summary>
/// Caso de uso para listar categorias com filtros opcionais.
/// </summary>
public class GetCategoriesUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesUseCase(
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<GetCategoriesResponse> ExecuteAsync(
        Guid? parentCategoryId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Category> categories;

        if (parentCategoryId.HasValue)
        {
            // Listar subcategorias de uma categoria específica
            categories = await _categoryRepository.GetByParentIdAsync(parentCategoryId.Value, includeInactive, cancellationToken);
        }
        else
        {
            // Listar todas ou apenas raiz (comportamento padrão: todas)
            categories = await _categoryRepository.GetAllAsync(includeInactive, cancellationToken);
        }

        var categoryDtos = _mapper.Map<IReadOnlyList<CategoryDto>>(categories);

        return new GetCategoriesResponse
        {
            Categories = categoryDtos,
            TotalCount = categoryDtos.Count
        };
    }
}
