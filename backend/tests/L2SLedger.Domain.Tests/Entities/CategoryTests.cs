using FluentAssertions;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Enums;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Tests.Entities;

/// <summary>
/// Testes da entidade Category.
/// Valida regras de negócio, validações e comportamento da hierarquia.
/// Atualizado conforme ADR-044 (CategoryType).
/// </summary>
public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldCreateCategoryWithDefaultValues()
    {
        // Arrange & Act
        var category = new Category("Alimentação", CategoryType.Expense, "Gastos com alimentação");

        // Assert
        category.Name.Should().Be("Alimentação");
        category.Description.Should().Be("Gastos com alimentação");
        category.Type.Should().Be(CategoryType.Expense);
        category.IsActive.Should().BeTrue();
        category.ParentCategoryId.Should().BeNull();
        category.Id.Should().NotBeEmpty();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithIncomeType_ShouldSetTypeCorrectly()
    {
        // Arrange & Act
        var category = new Category("Salário", CategoryType.Income, "Rendimentos de trabalho");

        // Assert
        category.Type.Should().Be(CategoryType.Income);
        category.Name.Should().Be("Salário");
    }

    [Fact]
    public void Constructor_WithExpenseType_ShouldSetTypeCorrectly()
    {
        // Arrange & Act
        var category = new Category("Transporte", CategoryType.Expense, "Gastos com transporte");

        // Assert
        category.Type.Should().Be(CategoryType.Expense);
        category.Name.Should().Be("Transporte");
    }

    [Fact]
    public void Constructor_WithInvalidType_ShouldThrowBusinessRuleException()
    {
        // Arrange & Act
        var act = () => new Category("Categoria", (CategoryType)99, "Descrição");

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*Tipo de categoria inválido*")
            .Where(ex => ex.Code == ErrorCodes.FIN_CATEGORY_INVALID_TYPE);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowBusinessRuleException()
    {
        // Arrange & Act
        var act = () => new Category("", CategoryType.Expense, "Descrição");

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*Nome da categoria é obrigatório*")
            .Where(ex => ex.Code == ErrorCodes.FIN_CATEGORY_INVALID_NAME);
    }

    [Fact]
    public void Constructor_WithWhiteSpaceName_ShouldThrowBusinessRuleException()
    {
        // Arrange & Act
        var act = () => new Category("   ", CategoryType.Expense, "Descrição");

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*Nome da categoria é obrigatório*")
            .Where(ex => ex.Code == ErrorCodes.FIN_CATEGORY_INVALID_NAME);
    }

    [Fact]
    public void Constructor_WithNameTooLong_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => new Category(longName, CategoryType.Expense, "Descrição");

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*não pode exceder 100 caracteres*")
            .Where(ex => ex.Code == ErrorCodes.FIN_CATEGORY_NAME_TOO_LONG);
    }

    [Fact]
    public void Constructor_WithParentCategoryId_ShouldSetParentCorrectly()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var category = new Category("Restaurantes", CategoryType.Expense, "Gastos em restaurantes", parentId);

        // Assert
        category.ParentCategoryId.Should().Be(parentId);
        category.Type.Should().Be(CategoryType.Expense);
        category.IsSubCategory().Should().BeTrue();
        category.IsRootCategory().Should().BeFalse();
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var category = new Category("Nome Antigo", CategoryType.Expense, "Descrição");
        var originalUpdatedAt = category.UpdatedAt;
        Thread.Sleep(10); // Garantir diferença no timestamp

        // Act
        category.UpdateName("Novo Nome");

        // Assert
        category.Name.Should().Be("Novo Nome");
        category.UpdatedAt.Should().NotBe(originalUpdatedAt);
        category.UpdatedAt.Should().BeAfter(category.CreatedAt);
    }

    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var category = new Category("Nome Original", CategoryType.Expense, "Descrição");

        // Act
        var act = () => category.UpdateName("");

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*Nome da categoria é obrigatório*")
            .Where(ex => ex.Code == ErrorCodes.FIN_CATEGORY_INVALID_NAME);
    }

    [Fact]
    public void UpdateName_WithNameTooLong_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var category = new Category("Nome Original", CategoryType.Expense, "Descrição");
        var longName = new string('B', 101);

        // Act
        var act = () => category.UpdateName(longName);

        // Assert
        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*não pode exceder 100 caracteres*")
            .Where(ex => ex.Code == ErrorCodes.FIN_CATEGORY_NAME_TOO_LONG);
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldUpdateDescriptionAndTimestamp()
    {
        // Arrange
        var category = new Category("Categoria", CategoryType.Income, "Descrição antiga");
        var originalUpdatedAt = category.UpdatedAt;
        Thread.Sleep(10);

        // Act
        category.UpdateDescription("Nova descrição");

        // Assert
        category.Description.Should().Be("Nova descrição");
        category.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldSetDescriptionToNull()
    {
        // Arrange
        var category = new Category("Categoria", CategoryType.Income, "Descrição existente");

        // Act
        category.UpdateDescription(null);

        // Assert
        category.Description.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = new Category("Categoria", CategoryType.Expense, "Descrição");
        category.IsActive.Should().BeTrue();

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var category = new Category("Categoria", CategoryType.Income, "Descrição");
        category.Deactivate();
        category.IsActive.Should().BeFalse();

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CanHaveSubCategories_ShouldReturnTrueForRootCategory()
    {
        // Arrange
        var rootCategory = new Category("Categoria Raiz", CategoryType.Expense, "Descrição");

        // Act & Assert
        rootCategory.CanHaveSubCategories().Should().BeTrue();
        rootCategory.IsRootCategory().Should().BeTrue();
    }

    [Fact]
    public void CanHaveSubCategories_ShouldReturnFalseForSubCategory()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var subCategory = new Category("Subcategoria", CategoryType.Expense, "Descrição", parentId);

        // Act & Assert
        subCategory.CanHaveSubCategories().Should().BeFalse();
        subCategory.IsSubCategory().Should().BeTrue();
    }

    [Fact]
    public void Type_ShouldBeImmutableAfterCreation()
    {
        // Arrange
        var category = new Category("Salário", CategoryType.Income, "Rendimentos");

        // Assert — Type não tem setter público
        typeof(Category).GetProperty("Type")!.SetMethod!.IsPrivate.Should().BeTrue();
    }
}
