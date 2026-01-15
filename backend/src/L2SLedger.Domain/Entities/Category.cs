using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Entities;

/// <summary>
/// Entidade de categoria financeira.
/// Suporta hierarquia de até 2 níveis (categoria pai e subcategoria).
/// Conforme ADR-029 (seed de dados).
/// </summary>
public class Category : Entity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// ID da categoria pai (null se for categoria raiz).
    /// Suporta apenas 1 nível de hierarquia (pai -> filho).
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }
    
    // Navigation property
    public Category? ParentCategory { get; private set; }
    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    // EF Core constructor
    protected Category() : base()
    {
        Name = string.Empty;
    }

    public Category(string name, string? description = null, Guid? parentCategoryId = null) : base()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("CAT_INVALID_NAME", "Nome da categoria é obrigatório");

        if (name.Length > 100)
            throw new BusinessRuleException("CAT_NAME_TOO_LONG", "Nome da categoria não pode exceder 100 caracteres");

        Name = name.Trim();
        Description = description?.Trim();
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("CAT_INVALID_NAME", "Nome da categoria é obrigatório");

        if (name.Length > 100)
            throw new BusinessRuleException("CAT_NAME_TOO_LONG", "Nome da categoria não pode exceder 100 caracteres");

        Name = name.Trim();
        UpdateTimestamp();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();
    }

    public bool IsRootCategory() => !ParentCategoryId.HasValue;

    public bool IsSubCategory() => ParentCategoryId.HasValue;

    /// <summary>
    /// Valida se a categoria pode ter subcategorias.
    /// Apenas categorias raiz (nível 0) podem ter filhas.
    /// </summary>
    public bool CanHaveSubCategories() => IsRootCategory();
}
