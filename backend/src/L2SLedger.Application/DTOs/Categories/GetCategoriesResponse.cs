namespace L2SLedger.Application.DTOs.Categories;

/// <summary>
/// Response para listagem de categorias.
/// </summary>
public record GetCategoriesResponse
{
    public required IReadOnlyList<CategoryDto> Categories { get; init; }
    public int TotalCount { get; init; }
}
