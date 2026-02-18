using FluentAssertions;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Enums;
using L2SLedger.Domain.Exceptions;
using Moq;

namespace L2SLedger.Application.Tests.UseCases.Categories;

public class DeactivateCategoryUseCaseTestsFixed
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly DeactivateCategoryUseCase _sut;

    public DeactivateCategoryUseCaseTestsFixed()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _sut = new DeactivateCategoryUseCase(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidId_ShouldDeactivateCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Alimentação", CategoryType.Expense, "Gastos com alimentação");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.CountSubCategoriesAsync(categoryId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await _sut.ExecuteAsync(categoryId);

        // Assert
        category.IsActive.Should().BeFalse();
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentId_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Categoria não encontrada*")
            .Where(ex => ex.Code == "FIN_CATEGORY_NOT_FOUND");
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyInactiveCategory_ShouldNotThrow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Categoria", CategoryType.Expense);
        category.Deactivate(); // Já está inativa

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.CountSubCategoriesAsync(categoryId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId);

        // Assert
        await act.Should().NotThrowAsync();
        category.IsActive.Should().BeFalse();
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveSubCategories_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parentCategory = new Category("Despesas", CategoryType.Expense, "Categoria pai");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentCategory);

        _categoryRepositoryMock
            .Setup(x => x.CountSubCategoriesAsync(parentId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2); // Tem 2 subcategorias ativas

        // Act
        var act = async () => await _sut.ExecuteAsync(parentId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Não é possível desativar uma categoria com subcategorias ativas*")
            .Where(ex => ex.Code == "VAL_BUSINESS_RULE_VIOLATION");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryUpdateAsync()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Categoria", CategoryType.Expense);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.CountSubCategoriesAsync(categoryId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await _sut.ExecuteAsync(categoryId);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Category>(c => !c.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category("Categoria", CategoryType.Expense);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(x => x.CountSubCategoriesAsync(categoryId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var act = async () => await _sut.ExecuteAsync(categoryId);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
