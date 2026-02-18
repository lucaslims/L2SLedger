namespace L2SLedger.Application.DTOs.Categories;

/// <summary>
/// DTO de categoria para respostas da API.
/// </summary>
public record CategoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Type { get; init; }
    public required bool IsActive { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public string? ParentCategoryName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
