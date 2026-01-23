namespace L2SLedger.Application.DTOs.Categories;

/// <summary>
/// Request para criação de categoria.
/// </summary>
public record CreateCategoryRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
}
