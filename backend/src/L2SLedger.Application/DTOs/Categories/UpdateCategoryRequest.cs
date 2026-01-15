namespace L2SLedger.Application.DTOs.Categories;

/// <summary>
/// Request para atualização de categoria.
/// </summary>
public record UpdateCategoryRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
