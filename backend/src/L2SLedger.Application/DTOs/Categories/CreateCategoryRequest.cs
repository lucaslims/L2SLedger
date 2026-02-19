namespace L2SLedger.Application.DTOs.Categories;

/// <summary>
/// Request para criação de categoria.
/// </summary>
public record CreateCategoryRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }

    /// <summary>
    /// Tipo da categoria: "Income" ou "Expense".
    /// Obrigatório para categorias raiz.
    /// Ignorado para subcategorias (herda do pai automaticamente, conforme ADR-044).
    /// </summary>
    public required string Type { get; init; }

    public Guid? ParentCategoryId { get; init; }
}
